using System;

namespace Core.Entities
{
    public class BusinessReceiveAddress
    {
        public Guid Id { get; set; }
        public Guid BusinessId { get; set; }
        public Guid BlockchainNetworkId { get; set; }
        public BlockchainNetwork BlockchainNetwork { get; set; } = null!;
        public string PublicAddress { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateUpdated { get; set; }
    }
}
