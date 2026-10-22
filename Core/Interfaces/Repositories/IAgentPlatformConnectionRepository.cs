namespace Core.Interfaces.Repositories
{
    public interface IAgentPlatformConnectionRepository
    {
        Task<AgentPlatformConnection?> GetByAgentAccountIdAndPlatformProfileUrlAsync(
            Guid agentAccountId, string platformProfileUrl, CancellationToken cancellationToken);

        Task<bool> SaveDemoConnectionAsync(Guid agentAccountId, string platformProfileUrl, DateTimeOffset expiresAt, CancellationToken cancellationToken);
    }
}
