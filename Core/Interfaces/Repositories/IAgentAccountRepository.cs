namespace Core.Interfaces.Repositories
{
    public interface IAgentAccountRepository
    {
        Task<AgentAccount> AddOrGetExistingAsync(AgentAccount agentAccount, CancellationToken cancellationToken);
        Task<AgentAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<AgentAccount?> GetByChainIdAndPublicAddressAsync(int chainId, string publicAddress, CancellationToken cancellationToken);
        Task SaveChangesAsync(CancellationToken cancellationToken);


        Task<AgentAccount?> GetAccessStateSnapshotAsync(int chainId, string publicAddress, CancellationToken cancellationToken);


        Task<AgentAccount?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken);

    }
}
