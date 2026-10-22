namespace Core.Entities.Agents
{
    public class AgentSpendingPolicyAsset
    {
        public Guid Id { get; set; }
        public Guid AgentSpendingPolicyId { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string? Issuer { get; set; }
        public decimal PerTransactionLimit { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset? DateUpdated { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public AgentSpendingPolicy AgentSpendingPolicy { get; set; } = null!;
    }
}
