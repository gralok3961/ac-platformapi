using static Core.Helpers.ApplicationEnum;

namespace Core.Entities.Agents
{
    public class AgentSigningAuthority
    {
        public Guid Id { get; set; }

        public Guid AgentAccountId { get; set; }

        public AgentSigningProvider Provider { get; set; }

        // Reference to the protected provider key; never credentials or private key material.
        public string ProviderKeyReference { get; set; } = string.Empty;

        public string PublicKey { get; set; } = string.Empty;

        public string DelegatedAuthorityIdentifier { get; set; } = string.Empty;

        public AgentSigningAuthorityStatus Status { get; set; } = AgentSigningAuthorityStatus.Provisioned;

        public DateTimeOffset? ActivatedAt { get; set; }

        public DateTimeOffset? RevokedAt { get; set; }

        public DateTimeOffset DateCreated { get; set; }

        public DateTimeOffset? DateUpdated { get; set; }

        public DateTimeOffset? DeletedAt { get; set; }

        public AgentAccount AgentAccount { get; set; } = null!;
    }
}
