namespace Core.Interfaces.Services
{
    public interface IPaymentCalculationService
    {
        CalculatedPricingSnapshotDto Calculate(BusinessPaymentContextDto businessContext, decimal checkoutAmount,
            PricingSnapshotDto pricingSnapshot);
    }
}


