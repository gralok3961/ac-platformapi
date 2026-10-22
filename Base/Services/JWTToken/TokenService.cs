using Base.Helpers.Dtos;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using static Base.Helpers.CommonEnum;

namespace Base.Services.JWTToken
{
    public class TokenService : ConfigService, ITokenService
    {
        private readonly JWTTokenConfig tokenConfig;
        public TokenService(IConfiguration config, JWTTokenConfig tokenConfig) : base(config)
        {
            this.tokenConfig = tokenConfig;
        }

        private string CreateByConfig(List<Claim> claims, JWTTokenConfig config)
        {
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = GetExpiry(config),
                SigningCredentials = GetCredential(config),
                Issuer = _config[tokenConfig.IssuerPath],
                Audience = string.IsNullOrWhiteSpace(config.AudiencePath) ? null : _config[config.AudiencePath]
            };

            return WriteToken(tokenDescriptor);
        }

        public string CreateByOnlyClaims(List<Claim> claims, JWTTokenConfig config)
        {
            return CreateByConfig(claims, config);
        }

        private SigningCredentials GetCredential(JWTTokenConfig config)
        {
            var signingKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config[config.SecretKeyPath]));

            return new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512Signature);
        }
        private static string WriteToken(SecurityTokenDescriptor descriptor)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(descriptor);

            return tokenHandler.WriteToken(token);
        }

        private DateTime GetExpiry([Optional] JWTTokenConfig tokenConfig)
        {
            var configTemp = tokenConfig ?? this.tokenConfig;

            var now = DateTimeOffset.UtcNow;

            var expiry = configTemp.AddType switch
            {
                DateTimeAdd.Days => now.AddDays(configTemp.ExpiryValue),
                DateTimeAdd.Months => now.AddMonths(configTemp.ExpiryValue),
                DateTimeAdd.Seconds => now.AddSeconds(configTemp.ExpiryValue),
                DateTimeAdd.Minutes => now.AddMinutes(configTemp.ExpiryValue),
                DateTimeAdd.Years => now.AddYears(configTemp.ExpiryValue),
                DateTimeAdd.Hours => now.AddHours(configTemp.ExpiryValue),
                _ => now.AddDays(1)
            };

            // Use UTC DateTime at the JWT boundary.
            return expiry.UtcDateTime;
        }
    }
}
