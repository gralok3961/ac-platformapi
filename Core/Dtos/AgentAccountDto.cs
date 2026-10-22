using System.Text.Json.Serialization;
using static Core.Helpers.ApplicationEnum;

namespace Core.Dtos
{
    public class IssueAgentAccountChallengeRequestDto
    {
        public Guid AppInstallationId { get; set; }
        public int ChainId { get; set; }
        public string PublicAddress { get; set; } = string.Empty;
    }
    public class IssueAgentAccountChallengeResponseDto
    {
        public string ChallengeId { get; set; } = string.Empty;
        public string Challenge { get; set; } = string.Empty;
        public DateTimeOffset ExpiresAt { get; set; }
    }
    public class AgentAccountChallengeDetailResponseDto
    {
        public string ChallengeId { get; set; } = string.Empty;
        public Guid AppInstallationId { get; set; }
        public int ChainId { get; set; }
        public string PublicAddress { get; set; } = string.Empty;
        public string Challenge { get; set; } = string.Empty;
        public AgentAccountChallengePurpose Purpose { get; set; }
        public AgentAccountChallengeStatus Status { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
    }

    //Provision
    public class ProvisionAgentAccountAccessRequestDto
    {
        [Required]
        [MaxLength(64)]
        public string ChallengeId { get; set; } = string.Empty;
        public Guid AppInstallationId { get; set; }
        [Required]
        [MaxLength(300)]
        public string PublicKey { get; set; } = string.Empty;
        [Required]
        public string Signature { get; set; } = string.Empty;
    }

    public class ProvisionAgentAccountAccessResponseDto
    {
        public Guid AgentAccountId { get; set; }
        public string PublicId { get; set; } = string.Empty;
        public string DelegatedAuthorityIdentifier { get; set; } = string.Empty;
    }

    public class AgentAccountActivationContextResponseDto
    {
        public Guid AgentAccountId { get; set; }
        public string PublicId { get; set; } = string.Empty;
        public Guid AppInstallationId { get; set; }
        public int ChainId { get; set; }
        public string PublicAddress { get; set; } = string.Empty;
        public Guid SigningAuthorityId { get; set; }
        public string DelegatedAuthorityIdentifier { get; set; } = string.Empty;
        public AgentSigningAuthorityStatus SigningAuthorityStatus { get; set; }
    }

    public class ActivateAgentSigningAuthorityRequestDto : IValidatableObject
    {
        public Guid AgentAccountId { get; set; }
        public Guid SigningAuthorityId { get; set; }

        public IEnumerable<ValidationResult> Validate(
            ValidationContext validationContext)
        {
            if (AgentAccountId == Guid.Empty)
            {
                yield return new ValidationResult(
                    "AgentAccountId is required.",
                    [nameof(AgentAccountId)]
                );
            }

            if (SigningAuthorityId == Guid.Empty)
            {
                yield return new ValidationResult(
                    "SigningAuthorityId is required.",
                    [nameof(SigningAuthorityId)]
                );
            }
        }
    }

    public class ActivateAgentSigningAuthorityResponseDto
    {
        public Guid AgentAccountId { get; set; }
        public string PublicId { get; set; } = string.Empty;
        public Guid SigningAuthorityId { get; set; }
        public AgentSigningAuthorityStatus Status { get; set; }
        public DateTimeOffset? ActivatedAt { get; set; }
    }

    //Transaction

    public class AgentTransactionExecutionContextResponseDto
    {
        public Guid AgentAccountId { get; set; }
        public int ChainId { get; set; }
        // rAGENT
        public string PublicAddress { get; set; } = string.Empty;
        public Guid SigningAuthorityId { get; set; }
        public AgentSigningProvider Provider { get; set; }
        // Azure Key Vault versioned key reference
        public string ProviderKeyReference { get; set; } = string.Empty;
        // Compressed secp256k1 public key
        public string PublicKey { get; set; } = string.Empty;
        // rKMS
        public string DelegatedAuthorityIdentifier { get; set; } = string.Empty;
        public AgentSigningAuthorityStatus SigningAuthorityStatus { get; set; }
    }
    // *** Spending Policy / Access State Fetch
    public class GetAgentAccountAccessStateInternalRequestDto
    {
        [Range(1, int.MaxValue)]
        public int ChainId { get; set; }
        [Required]
        public Guid AppInstallationId { get; set; }
        [Required]
        public string PublicAddress { get; set; } = string.Empty;
    }
    public class AgentSpendingPolicyAssetDto
    {
        public string CurrencyCode { get; set; } = string.Empty;
        public string? Issuer { get; set; }
        public string PerTransactionLimit { get; set; } = string.Empty;
    }

    public class AgentSpendingPolicyDto
    {
        public DateTimeOffset? ExpiresAt { get; set; }
        public List<AgentSpendingPolicyAssetDto> Assets { get; set; } = [];
    }

    public class GetAgentAccountAccessStateResponseDto
    {
        public string PublicId { get; set; } = string.Empty;
        public AgentSigningAuthorityStatus? SigningAuthorityStatus { get; set; }

        [Newtonsoft.Json.JsonConverter(
            typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public AgentSpendingPolicyState PolicyStatus { get; set; }

        public AgentSpendingPolicyDto? Policy { get; set; }
    }

    //Policy
    public class SaveAgentSpendingPolicyInternalRequestDto
    {
        public Guid AppInstallationId { get; set; }
        public int ChainId { get; set; }
        public string PublicAddress { get; set; } = string.Empty;
        public long Timestamp { get; set; }
        public string Nonce { get; set; } = string.Empty;
        public SaveAgentSpendingPolicyDto Policy { get; set; } = new();
    }

    public class SaveAgentSpendingPolicyDto
    {
        public DateTimeOffset ExpiresAt { get; set; }
        public List<SaveAgentSpendingPolicyAssetDto> Assets { get; set; } = [];
    }

    public class SaveAgentSpendingPolicyAssetDto
    {
        public string CurrencyCode { get; set; } = string.Empty;
        public string? Issuer { get; set; }
        public string PerTransactionLimit { get; set; } = string.Empty;
    }
}


