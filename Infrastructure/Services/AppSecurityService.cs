using Base.Helpers.Dtos;
using Infrastructure.Services.Auth;

namespace Infrastructure.Services
{
    public class AppSecurityService(IAppClientAuthService appClientAuthService) : IAppSecurityService
    {
        private readonly IAppClientAuthService _appClientAuthService = appClientAuthService;

        public ResponseDto<VerifyIosAppAttestResponse> CreateAppClientTokenResponse(Guid id)
        {
            var appClientToken = _appClientAuthService.IssueAccessToken(id);

            return new ResponseDto<VerifyIosAppAttestResponse>(
                new VerifyIosAppAttestResponse
                {
                    AppClientToken = appClientToken
                });
        }
    }
}
