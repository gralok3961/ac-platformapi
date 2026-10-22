using static Core.Helpers.ApplicationEnum;

namespace Core.Entities.Agents
{
    public class AgentAccount
    {
        public Guid Id { get; set; }
        public string PublicId { get; set; } = null!;
        public Guid AppInstallationId { get; set; }
        public int ChainId { get; set; }
        public string PublicAddress { get; set; } = string.Empty;
        public AgentAccountStatus Status { get; set; } = AgentAccountStatus.Active;
        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset? DateUpdated { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public AppInstallation.AppInstallation AppInstallation { get; set; } = null!;
        public ICollection<AgentSigningAuthority> SigningAuthorities { get; set; } = [];
        public ICollection<AgentSpendingPolicy> SpendingPolicies { get; set; } = [];
    }
}
