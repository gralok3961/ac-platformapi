namespace Core.Dtos
{
    public sealed class ConnectAgentPlatformDemoRequestDto
    {
        public string AgentId { get; set; } = string.Empty;
    }

    public sealed class VerifyIosAppAttestResponse
    {
        public string AppClientToken { get; set; } = string.Empty;
    }
}
