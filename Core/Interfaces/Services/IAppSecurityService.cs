namespace Core.Interfaces.Services
{
    public interface IAppSecurityService
    {
        ResponseDto<VerifyIosAppAttestResponse> CreateAppClientTokenResponse(Guid id);
    }
}
