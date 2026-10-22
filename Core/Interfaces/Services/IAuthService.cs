namespace Core.Interfaces.Services
{
    public interface IAppClientAuthService
    {
        string IssueAccessToken(Guid appInstallationId);
    }
}
