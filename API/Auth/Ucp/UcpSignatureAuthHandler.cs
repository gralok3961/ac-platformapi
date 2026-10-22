using System.Security.Claims;
using System.Text.Encodings.Web;

namespace API.Auth.Ucp
{
    public sealed class UcpSignatureAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder,
        IOptions<UcpSecurityOptions> security, IUcpProfileProvider profiles, UcpSignatureVerifier verifier)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        private readonly IOptions<UcpSecurityOptions> _security = security;
        private readonly IUcpProfileProvider _profiles = profiles;
        private readonly UcpSignatureVerifier _verifier = verifier;

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                var settings = _security.Value;
                var agentHeader = UcpSignatureVerifier.GetRequiredHeader(Context, "UCP-Agent");
                var profile = UcpAgentHeader.ParseProfile(agentHeader);
                UcpSignatureVerifier.GetRequiredHeader(Context, "Signature-Input");
                UcpSignatureVerifier.GetRequiredHeader(Context, "Signature");

                // Reject untrusted profiles before any DNS or HTTP request.
                if (!settings.TrustedPlatformProfiles.Contains(profile.AbsoluteUri, StringComparer.Ordinal))
                    return AuthenticateResult.Fail("UCP platform is not trusted.");

                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(Context.RequestAborted);
                timeout.CancelAfter(TimeSpan.FromSeconds(settings.ProfileTimeoutSeconds));

                var body = await ReadRequestBodyAsync(settings.MaximumRequestBodyBytes, timeout.Token);
                var publicKeys = await _profiles.GetPublicKeysAsync(profile, timeout.Token);
                _verifier.Verify(Context, publicKeys, body, settings);

                var claims = new[] { new Claim(HashClaimKeys.UCP_PLATFORM_PROFILE, profile.AbsoluteUri) };
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }
            catch (Exception)
            {
                // Do not expose headers, keys, request bodies or library diagnostics.
                return AuthenticateResult.Fail("A valid UCP platform HTTP message signature is required.");
            }
        }

        private async Task<byte[]> ReadRequestBodyAsync(int maximumBytes, CancellationToken cancellationToken)
        {
            if (Request.ContentLength > maximumBytes)
                throw new FormatException("UCP request body exceeds the configured limit.");

            Request.EnableBuffering(bufferThreshold: maximumBytes, bufferLimit: maximumBytes);
            using var body = new MemoryStream();
            var buffer = new byte[8192];

            try
            {
                int count;
                while ((count = await Request.Body.ReadAsync(buffer, cancellationToken)) != 0)
                {
                    if (body.Length + count > maximumBytes)
                        throw new FormatException("UCP request body exceeds the configured limit.");

                    body.Write(buffer, 0, count);
                }

                return body.ToArray();
            }
            finally
            {
                Request.Body.Position = 0;
            }
        }
    }
}
