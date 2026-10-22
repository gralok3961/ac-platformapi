using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging.Abstractions;
using NSign.Http;
using NSign.Signatures;
using StructuredFieldValues;

namespace API.Auth.Ucp
{
    public sealed class UcpSignatureVerifier(TimeProvider clock)
    {
        private readonly TimeProvider _clock = clock;

        public void Verify(HttpContext context, JsonElement publicKeys, byte[] body, UcpSecurityOptions options)
        {
            try
            {
                var message = CreateRequestContext(context, options.PublicOrigin);
                var requiredComponents = GetRequiredComponents(context.Request, body.Length > 0, message.HasQuery);

                ValidateSignatureHeaders(context);
                ValidateContentDigest(context, body);
                ValidateIdempotencyKey(context, requiredComponents);

                foreach (var candidate in message.SignaturesForVerification)
                {
                    var parameters = candidate.SignatureParams;
                    if (candidate.Signature.Length != 64 || parameters.Tag != null || parameters.Algorithm != null)
                        continue;

                    if (!HasRequiredComponents(parameters, requiredComponents) || !IsFresh(parameters, options))
                        continue;

                    using var key = GetPublicKey(publicKeys, parameters.KeyId);
                    if (key != null && key.VerifyData(
                        message.GetSignatureInput(parameters, out _).Span,
                        candidate.Signature.Span,
                        HashAlgorithmName.SHA256,
                        DSASignatureFormat.IeeeP1363FixedFieldConcatenation))
                        return;
                }
            }
            catch (Exception)
            {
                // Never expose signature, key or body values in authentication diagnostics.
                throw InvalidSignature();
            }

            throw InvalidSignature();
        }

        private static RequestSignatureContext CreateRequestContext(HttpContext context, string publicOrigin)
        {
            var authority = context.Request.Host.ToUriComponent().ToLowerInvariant();
            if (authority.EndsWith(":443", StringComparison.Ordinal))
            {
                authority = authority[..^4];
            }

            if (authority != new Uri(publicOrigin).Authority.ToLowerInvariant())
                throw InvalidSignature();

            // Preserve the original encoded path and query; never trust forwarded headers here.
            var rawTarget = context.Features.Get<IHttpRequestFeature>()?.RawTarget;
            if (string.IsNullOrEmpty(rawTarget))
            {
                rawTarget = context.Request.PathBase.ToUriComponent()
                    + context.Request.Path.ToUriComponent() + context.Request.QueryString.Value;
            }

            if (!rawTarget.StartsWith('/') || rawTarget.StartsWith("//") ||
                rawTarget.Contains('#') || rawTarget.Any(c => c < 0x21 || c > 0x7e))
                throw InvalidSignature();

            var queryStart = rawTarget.IndexOf('?');
            var path = queryStart < 0 ? rawTarget : rawTarget[..queryStart];
            var query = queryStart < 0 ? "?" : rawTarget[queryStart..];
            return new RequestSignatureContext(context, authority, path, query, queryStart >= 0);
        }

        private static void ValidateSignatureHeaders(HttpContext context)
        {
            var input = GetRequiredHeader(context, "Signature-Input");
            var signature = GetRequiredHeader(context, "Signature");

            if (SfvParser.ParseDictionary(input, out var inputs).HasValue || inputs.Count is < 1 or > 4 ||
                SfvParser.ParseDictionary(signature, out var signatures).HasValue ||
                !inputs.Keys.ToHashSet().SetEquals(signatures.Keys))
                throw InvalidSignature();
        }

        private static List<string> GetRequiredComponents(HttpRequest request, bool hasBody, bool hasQuery)
        {
            var required = new List<string> { "@method", "@authority", "@path", "ucp-agent" };
            if (hasQuery)
            {
                required.Add("@query");
            }

            if (!HttpMethods.IsGet(request.Method) || request.Headers.ContainsKey("Idempotency-Key"))
            {
                required.Add("idempotency-key");
            }

            foreach (var header in new[] { "content-digest", "content-type", "content-encoding", "signature-agent" })
            {
                if (request.Headers.ContainsKey(header) || hasBody && header is "content-digest" or "content-type")
                {
                    required.Add(header);
                }
            }

            return required;
        }

        private static bool HasRequiredComponents(SignatureParamsComponent parameters, IEnumerable<string> required)
        {
            // Parameterized variants (;req, ;key, etc.) cannot replace a whole request field.
            return required.All(name => parameters.Components.Any(component =>
                component.Equals(name.StartsWith('@')
                    ? (object)new DerivedComponent(name)
                    : new HttpHeaderComponent(name))));
        }

        private bool IsFresh(SignatureParamsComponent parameters, UcpSecurityOptions options)
        {
            var now = _clock.GetUtcNow();
            return parameters.Created != null && parameters.Expires != null
                && parameters.Created <= now.AddSeconds(options.ClockSkewSeconds)
                && parameters.Created >= now.AddSeconds(-options.SignatureMaxAgeSeconds - options.ClockSkewSeconds)
                && parameters.Expires > now
                && parameters.Expires > parameters.Created
                && parameters.Expires - parameters.Created <= TimeSpan.FromSeconds(options.SignatureMaxAgeSeconds);
        }

        private static void ValidateContentDigest(HttpContext context, byte[] body)
        {
            if (body.Length == 0 && !context.Request.Headers.ContainsKey("Content-Digest"))
                return;

            var header = GetRequiredHeader(context, "Content-Digest");
            if (SfvParser.ParseDictionary(header, out var digests).HasValue ||
                !digests.TryGetValue("sha-256", out var digest) || digest.Value is not ReadOnlyMemory<byte> bytes ||
                !CryptographicOperations.FixedTimeEquals(bytes.Span, SHA256.HashData(body)))
                throw InvalidSignature();
        }

        private static void ValidateIdempotencyKey(HttpContext context, List<string> required)
        {
            if (!required.Contains("idempotency-key"))
                return;

            var key = GetRequiredHeader(context, "Idempotency-Key");
            if (key.Length is < 22 or > 256 || key.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not ('-' or '_')))
                throw InvalidSignature();
        }

        private static ECDsa? GetPublicKey(JsonElement publicKeys, string? keyId)
        {
            if (string.IsNullOrEmpty(keyId) || publicKeys.ValueKind != JsonValueKind.Array)
                return null;

            var matches = publicKeys.EnumerateArray().Where(key => GetText(key, "kid") == keyId).ToArray();
            if (matches.Length != 1)
                return null;

            var key = matches[0];
            var fields = new[] { "kty", "crv", "alg", "use", "x", "y" };
            if (fields.Any(name => key.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.String) ||
                GetText(key, "kty") != "EC" || GetText(key, "crv") != "P-256" ||
                GetText(key, "alg") is not (null or "ES256") || GetText(key, "use") is not (null or "sig") ||
                key.TryGetProperty("d", out _))
                return null;

            if (key.TryGetProperty("key_ops", out var operations) &&
                (operations.ValueKind != JsonValueKind.Array ||
                 !operations.EnumerateArray().Any(value => value.ValueKind == JsonValueKind.String && value.GetString() == "verify")))
                return null;

            return ECDsa.Create(new ECParameters
            {
                Curve = ECCurve.NamedCurves.nistP256,
                Q = new ECPoint { X = ReadCoordinate(GetText(key, "x")), Y = ReadCoordinate(GetText(key, "y")) }
            });
        }

        private static string? GetText(JsonElement value, string name)
        {
            return value.TryGetProperty(name, out var field) && field.ValueKind == JsonValueKind.String
                ? field.GetString() : null;
        }

        private static byte[] ReadCoordinate(string? value)
        {
            if (value == null || !Regex.IsMatch(value, "^[A-Za-z0-9_-]{43}$", RegexOptions.NonBacktracking))
            {
                throw InvalidSignature();
            }

            var bytes = WebEncoders.Base64UrlDecode(value);
            if (bytes.Length != 32 || WebEncoders.Base64UrlEncode(bytes) != value)
                throw InvalidSignature();

            return bytes;
        }

        internal static string GetRequiredHeader(HttpContext context, string name)
        {
            var values = context.Request.Headers[name];
            if (values.Count != 1 || string.IsNullOrWhiteSpace(values[0]) || values[0]!.Length > 8192 ||
                values[0]!.Any(c => c < 0x20 && c != '\t' || c > 0x7e))
                throw InvalidSignature();

            return values[0]!;
        }

        private static FormatException InvalidSignature() => new("A valid platform HTTP message signature is required.");

        // NSign handles RFC 9421 parsing and signature-base serialization.
        private sealed class RequestSignatureContext(HttpContext context, string authority, string path, string query, bool hasQuery)
            : MessageContext(NullLogger.Instance, new HttpFieldOptions())
        {
            public bool HasQuery => hasQuery;
            public override bool HasResponse => false;
            public override CancellationToken Aborted => context.RequestAborted;
            public override void AddHeader(string name, string value) => throw new NotSupportedException();
            public override IEnumerable<string> GetHeaderValues(string name) => context.Request.Headers[name].Select(value => value!.Trim(' ', '\t'));
            public override IEnumerable<string> GetRequestHeaderValues(string name) => GetHeaderValues(name);
            public override IEnumerable<string> GetTrailerValues(string name) => throw new NotSupportedException();
            public override IEnumerable<string> GetRequestTrailerValues(string name) => throw new NotSupportedException();
            public override IEnumerable<string> GetQueryParamValues(string name) => context.Request.Query[name].Select(value => value!);
            public override string? GetDerivedComponentValue(DerivedComponent component) => component.ComponentName switch
            {
                "@method" => context.Request.Method,
                "@authority" => authority,
                "@path" => path,
                "@query" => query,
                _ => throw new NotSupportedException()
            };
        }
    }
}
