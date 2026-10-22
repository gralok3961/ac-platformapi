
namespace Infrastructure.Data.Repositories
{
    public class BusinessPaymentContextRepository(ApplicationDbContext dbContext) : IBusinessPaymentContextRepository
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        public async Task<BusinessPaymentContextDto?> GetPaymentContextByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken)
        {
            return await _dbContext.Businesses
                .AsNoTracking()
                .Where(b => b.Id == businessId && b.IsActive && b.DeletedAt == null)
                .Select(b => new BusinessPaymentContextDto
                {
                    ActiveNetworks = b.ReceiveAddresses
                        .Where(r => r.IsActive)
                        .Select(r => r.BlockchainNetwork.Alias)
                        .ToList(),

                    ActiveTokens = b.AllowTokens
                        .Where(t => t.IsActive &&
                                    t.IsOnChainEnabled &&
                                    t.IsPaymentAccepted &&
                                    b.ReceiveAddresses.Any(r =>
                                        r.IsActive &&
                                        r.BlockchainNetworkId == t.BlockchainNetworkId))
                        .Select(t => new BusinessPaymentTokenDto
                        {
                            Symbol = t.DiscoverableToken.Symbol,
                            CurrencyCode = t.DiscoverableToken.CurrencyCode,
                            IssuerAddress = t.DiscoverableToken.IssuerAddress,
                            Decimals = t.DiscoverableToken.Decimals,
                            NetworkAlias = t.BlockchainNetwork.Alias,
                            IsNative = t.DiscoverableToken.IsNative
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
