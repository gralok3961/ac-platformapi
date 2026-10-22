namespace Core.Interfaces.Repositories
{
    public interface IAgentSpendingPolicyRepository
    {
        Task AddAsync(AgentSpendingPolicy policy, CancellationToken cancellationToken);
        Task SaveChangesAsync(CancellationToken cancellationToken);


        Task<AgentSpendingPolicy?> GetCurrentWithAssetsAsync(Guid agentAccountId, CancellationToken cancellationToken);
    }
}
