namespace Core.Interfaces.Repositories
{
    public interface IAgentSpendingPolicyAssetRepository
    {
        Task AddAsync(AgentSpendingPolicyAsset asset, CancellationToken cancellationToken);
    }
}
