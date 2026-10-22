namespace Core.Entities
{
    public class BusinessAllowToken
    {
        public Guid Id { get; set; }

        // FK
        public Guid BusinessId { get; set; }
        public Guid BlockchainNetworkId { get; set; }
        public Guid DiscoverableTokenId { get; set; }

        // Business policy
        public bool IsActive { get; set; } = true;
        public bool IsOnChainEnabled { get; set; } = false;
        public bool IsPaymentAccepted { get; set; } = true;
        // UI
        public int SortOrder { get; set; } = 0;

        // Audit
        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateUpdated { get; set; }

        // Navigation properties (EF Core)
        public Business Business { get; set; } = null!;
        public BlockchainNetwork BlockchainNetwork { get; set; } = null!;
        public DiscoverableToken DiscoverableToken { get; set; } = null!;
    }
}