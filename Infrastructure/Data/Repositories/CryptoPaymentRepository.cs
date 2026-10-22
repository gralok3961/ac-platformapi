
namespace Infrastructure.Data.Repositories
{
    public class CryptoPaymentRepository(ApplicationDbContext dbContext) : ICryptoPaymentRepository
    {
        private readonly ApplicationDbContext _dbContext = dbContext;
        public async Task<CryptoPayment> CreateCryptoPayment(CryptoPayment cryptoPayment)
        {
            _dbContext.CryptoPayments.Add(cryptoPayment);

            await _dbContext.SaveChangesAsync();

            return cryptoPayment;
        }

        public async Task<CryptoPayment?> GetAgentPaymentByCheckoutIdAsync(Guid businessId, string checkoutId, CancellationToken cancellationToken)
        {
            return await _dbContext.CryptoPayments.AsNoTracking().FirstOrDefaultAsync(x =>
                    x.BusinessId == businessId &&
                    x.ExternalReference == checkoutId &&
                    x.PaymentSource == nameof(CryptoPaymentSource.AGENT),
                    cancellationToken);
        }
    }
}
