namespace Base.Custom.HashCross.Entities
{
    public class BlockchainNetwork
    {
        public Guid Id { get; set; }

        // NOT NULL (DB + Code)
        public string NetworkTitle { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;

        public string RpcUrl1 { get; set; } = string.Empty;
        public string SocketUrl1 { get; set; } = string.Empty;

        public int ChainId { get; set; }

        public string Alias { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;

        public string ExplorerUrl1 { get; set; } = string.Empty;

        // Optional columns (DB allows NULL, code uses Empty)
        public string RpcUrl2 { get; set; } = string.Empty;
        public string RpcUrl3 { get; set; } = string.Empty;

        public string SocketUrl2 { get; set; } = string.Empty;
        public string SocketUrl3 { get; set; } = string.Empty;

        public string TestRpcUrl { get; set; } = string.Empty;
        public string TestSocketUrl { get; set; } = string.Empty;

        public string ExplorerUrl2 { get; set; } = string.Empty;

        // DB-managed timestamps
        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateUpdated { get; set; }
    }
}



