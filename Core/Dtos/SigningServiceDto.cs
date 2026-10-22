using Newtonsoft.Json;
using static Core.Helpers.ApplicationEnum;

namespace Core.Dtos
{
    public class ProvisionAgentSigningAuthorityRequestDto
    {
        [JsonProperty("agentAccountId")]
        public Guid AgentAccountId { get; set; }

        [JsonProperty("chainId")]
        public int ChainId { get; set; }
    }

    public class ProvisionAgentSigningAuthorityResponseDto
    {
        public AgentSigningProvider Provider { get; set; }
        public string ProviderKeyReference { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public string DelegatedAuthorityIdentifier { get; set; } = string.Empty;
    }
}

