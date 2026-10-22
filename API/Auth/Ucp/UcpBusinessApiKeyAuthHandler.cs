using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using static Base.Custom.HashCross.Helpers.HashCrossEnum.HashCrossProtocol;

namespace API.Auth.Ucp
{
    public sealed class UcpBusinessApiKeyAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder,
        IConfiguration configuration) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string SchemeName = "UcpBusinessApiKey";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("X-API-Key", out var providedKey))
                return Task.FromResult(AuthenticateResult.NoResult());

            var expectedKey = configuration["AuthAPIs:UcpBusinessCredential"];
            if (string.IsNullOrWhiteSpace(expectedKey) || expectedKey.Length is < 32 or > 4096 ||
                providedKey.Count != 1 || string.IsNullOrWhiteSpace(providedKey[0]) || providedKey[0]!.Length > 4096)
                return Task.FromResult(AuthenticateResult.Fail("A valid UCP business API key is required."));

            var provided = Encoding.UTF8.GetBytes(providedKey[0]!);
            var expected = Encoding.UTF8.GetBytes(expectedKey);
            bool matches;
            try { matches = CryptographicOperations.FixedTimeEquals(SHA256.HashData(provided), SHA256.HashData(expected)); }
            finally { CryptographicOperations.ZeroMemory(provided); CryptographicOperations.ZeroMemory(expected); }
            if (!matches)
                return Task.FromResult(AuthenticateResult.Fail("Invalid UCP business API key."));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, CommonApiAuth.NameIdentifier),
                new Claim(ClaimTypes.Name, CommonApiAuth.DisplayName),
                new Claim(CommonApiAuth.ClientTypeClaim, CommonApiAuth.ClientTypeValue)
            };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
        }
    }
}
