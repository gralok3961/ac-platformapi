using Base.Helpers;

namespace Core.Interfaces.Repositories
{
    public interface ICryptoPaymentRepository
    {
        Task<CryptoPayment> CreateCryptoPayment(CryptoPayment cryptoPayment);

        Task<CryptoPayment?> GetAgentPaymentByCheckoutIdAsync(Guid businessId, string checkoutId, CancellationToken cancellationToken);
    }
}
