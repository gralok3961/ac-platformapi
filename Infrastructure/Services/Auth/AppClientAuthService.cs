using System.Security.Claims;
using Base.Helpers.Dtos;
using Base.Services.JWTToken;

namespace Infrastructure.Services.Auth
{
    public class AppClientAuthService(ITokenService tokenService, JWTTokenConfig appClientTokenConfig) : IAppClientAuthService
    {
        private readonly ITokenService _tokenService = tokenService;
        private readonly JWTTokenConfig _tokenConfig = appClientTokenConfig;
        public string IssueAccessToken(Guid appInstallationId)
        {
            var claims = new List<Claim>
            {
                new(HashClaimKeys.APP_INSTALLATION_ID, appInstallationId.ToString()),
                new(HashClaimKeys.APP_CLIENT_TYPE, HashAppClientType.WALLET)
            };

            return _tokenService.CreateByOnlyClaims(claims, _tokenConfig);
        }
    }
}