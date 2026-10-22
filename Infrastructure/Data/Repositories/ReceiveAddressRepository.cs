namespace Infrastructure.Data.Repositories
{
    public class ReceiveAddressRepository(ApplicationDbContext dbContext) : IReceiveAddressRepository
    {
        private readonly ApplicationDbContext _dbContext = dbContext;
        public async Task<BusinessReceiveAddress?> GetActiveByBusinessAndChainAsync(Guid businessId, int chainId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.BusinessReceiveAddresses.Include(x => x.BlockchainNetwork).FirstOrDefaultAsync(x =>
                    x.BusinessId == businessId &&
                    x.BlockchainNetwork.ChainId == chainId &&
                    x.IsActive,
                    cancellationToken);
        }



    }
}
