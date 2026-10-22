namespace Core.Entities.Agents
{
    public class AgentSpendingPolicy
    {
        public Guid Id { get; set; }
        public Guid AgentAccountId { get; set; }
        public DateTimeOffset? ExpiresAt { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset? DateUpdated { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public AgentAccount AgentAccount { get; set; } = null!;
        public ICollection<AgentSpendingPolicyAsset> Assets { get; set; } = [];
    }
}
