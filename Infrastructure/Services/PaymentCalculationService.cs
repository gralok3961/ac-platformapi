using System.Globalization;

namespace Infrastructure.Services
{
    public class PaymentCalculationService : IPaymentCalculationService
    {
        public PaymentCalculationService() { }
        public CalculatedPricingSnapshotDto Calculate(BusinessPaymentContextDto businessContext,
        decimal checkoutAmount, PricingSnapshotDto pricingSnapshot)
        {
            if (businessContext == null)
                throw new ArgumentNullException(nameof(businessContext));

            if (pricingSnapshot == null)
                throw new ArgumentNullException(nameof(pricingSnapshot));

            //Update to only add supported tokens
            var tokens = businessContext.ActiveTokens.ToList();
            var calculated = BuildCalculatedSnapshot(tokens, checkoutAmount, pricingSnapshot);

            return calculated;
        }

        // ============================
        // Deterministic Calculator
        // ============================
        private static CalculatedPricingSnapshotDto BuildCalculatedSnapshot(List<BusinessPaymentTokenDto> tokens,
            decimal checkoutAmount, PricingSnapshotDto pricingSnapshot)
        {
            var result = new CalculatedPricingSnapshotDto
            {
                BaseCurrency = pricingSnapshot.BaseCurrency,
                Provider = pricingSnapshot.Provider,
                CapturedAt = pricingSnapshot.CapturedAt,
                Tokens = []
            };

            foreach (var token in tokens)
            {
                var key = token.Symbol;

                if (!pricingSnapshot.Prices.TryGetValue(key, out var price)) continue;

                if (price <= 0) continue;

                var required = checkoutAmount / price;
                // Always round up to the asset's supported precision
                required = RoundUp(required, token.Decimals);

                result.Tokens.Add(new CalculatedTokenPricingDto
                {
                    NetworkAlias = token.NetworkAlias,
                    Symbol = token.Symbol,
                    CurrencyCode = token.CurrencyCode,
                    IssuerAddress = token.IssuerAddress,
                    UnitPrice = price.ToString(CultureInfo.InvariantCulture),
                    RequiredAmount = required.ToString(CultureInfo.InvariantCulture),
                    Decimals = token.Decimals,
                    IsNative = token.IsNative
                });
            }

            return result;
        }

        private static decimal RoundUp(decimal value, int decimals)
        {
            if (decimals < 0 || decimals > 28)
                throw new ArgumentOutOfRangeException(nameof(decimals));

            decimal factor = 1m;

            for (var i = 0; i < decimals; i++)
                factor *= 10m;

            return Math.Ceiling(value * factor) / factor;
        }
    }
}
