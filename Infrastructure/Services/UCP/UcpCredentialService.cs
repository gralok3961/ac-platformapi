using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services.UCP
{
    public class UcpCredentialService(IConfiguration configuration) : IUcpCredentialService
    {
        private readonly SigningCredentials _credentials = CreateCredentials(configuration);

        private readonly string _issuer = configuration["UcpSecurity:PublicOrigin"] ?? throw new InvalidOperationException("UCP issuer is missing.");

        private static SigningCredentials CreateCredentials(IConfiguration config)
        {
            var secret = config[HashCrossConfigPath.AUTH_UCPCREDENTIAL];

            if (string.IsNullOrWhiteSpace(secret))
                throw new InvalidOperationException("UCP credential secret is missing.");

            var keyBytes = Convert.FromBase64String(secret);

            if (keyBytes.Length != 32) throw new InvalidOperationException("Invalid UCP credential key length.");

            return new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);
        }

        public UcpCredentialResult Issue(string agentPublicId, string businessId, string checkoutId, string platformProfileUrl)
        {
            if (string.IsNullOrWhiteSpace(agentPublicId) ||
                string.IsNullOrWhiteSpace(businessId) ||
                string.IsNullOrWhiteSpace(checkoutId) ||
                string.IsNullOrWhiteSpace(platformProfileUrl))
                throw new ArgumentException("Credential binding is incomplete.");

            var now = DateTimeOffset.UtcNow;
            var expiry = now.AddMinutes(5);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new("agent_public_id", agentPublicId),
                new("business_id", businessId),
                new("checkout_id", checkoutId),
                new("platform_profile_url", platformProfileUrl),
                new("token_use", "ucp_payment_credential")
            };

            var jwt = new JwtSecurityToken(
                issuer: _issuer,
                audience: "com.hashtrion.pay",
                claims: claims,
                notBefore: now.UtcDateTime,
                expires: expiry.UtcDateTime,
                signingCredentials: _credentials);

            jwt.Header["typ"] = "hashtrion-pay+jwt";

            return new UcpCredentialResult
            {
                Type = "hashtrion_authorization",
                Token = new JwtSecurityTokenHandler().WriteToken(jwt),
                Expiry = expiry
            };
        }

        public (string AgentPublicId, string Jti) Validate(string token, string businessId, string checkoutId, string platformProfileUrl)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler
                {
                    MapInboundClaims = false
                };

                var parameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = _credentials.Key,
                    RequireSignedTokens = true,
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = "com.hashtrion.pay",
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidTypes = ["hashtrion-pay+jwt"]
                };

                var principal = handler.ValidateToken(token, parameters, out _);

                var agentPublicId = principal.FindFirst("agent_public_id")?.Value;

                if (!string.Equals(principal.FindFirst("business_id")?.Value, businessId, StringComparison.Ordinal) ||
                    !string.Equals(principal.FindFirst("checkout_id")?.Value, checkoutId, StringComparison.Ordinal) ||
                    !string.Equals(principal.FindFirst("platform_profile_url")?.Value, platformProfileUrl, StringComparison.Ordinal) ||
                    principal.FindFirst("token_use")?.Value != "ucp_payment_credential" ||
                    string.IsNullOrWhiteSpace(agentPublicId))
                {
                    throw new SecurityTokenException("Invalid payment credential claims.");
                }

                var jti = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

                if (string.IsNullOrWhiteSpace(jti))
                    throw new SecurityTokenException("Missing credential identifier.");

                return (agentPublicId, jti);
            }
            catch (Exception ex) when (
                ex is SecurityTokenException or ArgumentException or FormatException)
            {
                throw new UcpPaymentRequestException(
                    401, "invalid_payment_credential", "Payment credential is invalid or expired.");
            }
        }
    }
}