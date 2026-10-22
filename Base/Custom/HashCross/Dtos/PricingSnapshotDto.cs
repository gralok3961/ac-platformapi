namespace Base.Custom.HashCross.Dtos
{
    public sealed class PricingSnapshotDto
    {
        public string BaseCurrency { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public DateTimeOffset CapturedAt { get; set; }
        public Dictionary<string, decimal> Prices { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }

    public sealed class CalculatedPricingSnapshotDto
    {
        public string BaseCurrency { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public DateTimeOffset CapturedAt { get; set; }

        public List<CalculatedTokenPricingDto> Tokens { get; set; } = [];
    }

    public sealed class CalculatedTokenPricingDto
    {
        public string NetworkAlias { get; set; } = string.Empty;

        // UI / display
        public string Symbol { get; set; } = string.Empty;

        // Exact blockchain asset identity
        public string? CurrencyCode { get; set; }
        public string? IssuerAddress { get; set; }

        public string UnitPrice { get; set; } = string.Empty;
        public string RequiredAmount { get; set; } = string.Empty;

        public int Decimals { get; set; }
        public bool IsNative { get; set; }
    }
}
