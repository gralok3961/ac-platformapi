namespace Infrastructure.Data.Repositories
{
    public class AgentPlatformConnectionRepository(ApplicationDbContext dbContext) : IAgentPlatformConnectionRepository
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        public async Task<AgentPlatformConnection?> GetByAgentAccountIdAndPlatformProfileUrlAsync(
            Guid agentAccountId, string platformProfileUrl, CancellationToken cancellationToken)
        {
            return await _dbContext.AgentPlatformConnections.AsNoTracking().FirstOrDefaultAsync(
                x => x.AgentAccountId == agentAccountId && x.PlatformProfileUrl == platformProfileUrl,
                cancellationToken);
        }

        public async Task<bool> SaveDemoConnectionAsync(Guid agentAccountId, string platformProfileUrl, DateTimeOffset expiresAt, CancellationToken cancellationToken)
        {
            var connection = await _dbContext.AgentPlatformConnections.FirstOrDefaultAsync(x =>
            x.AgentAccountId == agentAccountId && x.PlatformProfileUrl == platformProfileUrl, cancellationToken);

            if (connection == null)
            {
                connection = new AgentPlatformConnection
                {
                    Id = Guid.NewGuid(),
                    AgentAccountId = agentAccountId,
                    PlatformProfileUrl = platformProfileUrl,
                    Status = AgentPlatformConnectionStatus.Active,
                    ExpiresAt = expiresAt
                };

                await _dbContext.AgentPlatformConnections.AddAsync(connection, cancellationToken);
            }
            else
            {
                if (connection.Status != AgentPlatformConnectionStatus.Active)
                    return false;

                connection.ExpiresAt = expiresAt;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
