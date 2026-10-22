namespace Core.Interfaces.Repositories
{
    public interface IReceiveAddressRepository
    {
                                
        Task<BusinessReceiveAddress?> GetActiveByBusinessAndChainAsync(Guid businessId, int chainId, CancellationToken cancellationToken = default);

        
    }
}
