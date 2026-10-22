namespace Base.Custom.HashCross.Dtos
{
    public class SupportedFiatCurrencyDto
    {
        public Guid Id { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string CurrencyName { get; set; } = string.Empty;
        public int DecimalPlaces { get; set; }
    }
}

