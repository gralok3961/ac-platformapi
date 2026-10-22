namespace Core.Interfaces.Services.UCP
{
    public interface IUcpCredentialService
    {
        UcpCredentialResult Issue(string agentPublicId, string businessId, string checkoutId, string platformProfileUrl);

        (string AgentPublicId, string Jti) Validate(string token, string businessId, string checkoutId, string platformProfileUrl);
    }
}