using static Base.Custom.HashCross.Helpers.HashCrossEnum;

namespace Base.Custom.HashCross.Entities
{
    public class DiscoverableToken
    {
        public Guid Id { get; set; }

        public string Environment { get; set; } = string.Empty;

        public string Network { get; set; } = string.Empty;

        public string Symbol { get; set; } = string.Empty;
        public string? CurrencyCode { get; set; }

        public string Name { get; set; } = string.Empty;

        public string IssuerAddress { get; set; }

        public bool IsNative { get; set; }

        public bool IsStablecoin { get; set; }

        public string AboutText { get; set; } = string.Empty;
        public string IconUrl { get; set; } = string.Empty;

        public int Decimals { get; set; } = 6;

        public string IssuerName { get; set; } = string.Empty;
        public string IssuerDomain { get; set; } = string.Empty;
        public string IssuerDescription { get; set; } = string.Empty;

        public decimal? IssuerFeePercent { get; set; }
        public decimal? MarketCap { get; set; }
        public decimal? TotalSupply { get; set; }
        public decimal? CirculatingSupply { get; set; }

        public TokenStatus Status { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateUpdated { get; set; }
    }
}