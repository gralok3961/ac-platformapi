namespace Base.Custom.HashCross.Dtos
{
    public class DiscoverableTokenDto
    {
        public Guid Id { get; set; }

        public string Network { get; set; } = default!;
        public string Symbol { get; set; } = default!;
        public string? CurrencyCode { get; set; }
        public string Name { get; set; } = default!;
        public string IssuerAddress { get; set; } = default!;
        public string? AboutText { get; set; }
        public string IconUrl { get; set; } = default!;
        public int Decimals { get; set; }
        public bool IsNative { get; set; }
        public bool IsStablecoin { get; set; }
        public string? IssuerName { get; set; }
        public string? IssuerDomain { get; set; }
        public string? IssuerDescription { get; set; }
        public decimal? IssuerFeePercent { get; set; }

        public decimal? MarketCap { get; set; }
        public decimal? TotalSupply { get; set; }
        public decimal? CirculatingSupply { get; set; }
        public decimal? PriceInNative { get; set; }
    }
}