namespace Core.Dtos
{
    public sealed class BusinessPaymentTokenDto
    {
        public string Symbol { get; set; } = string.Empty;
        public string? CurrencyCode { get; set; }
        public string? IssuerAddress { get; set; }
        public string NetworkAlias { get; set; } = string.Empty;
        public int Decimals { get; set; }
        public bool IsNative { get; set; }
    }
}
