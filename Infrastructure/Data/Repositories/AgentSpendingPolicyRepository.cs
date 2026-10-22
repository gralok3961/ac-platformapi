namespace Infrastructure.Data.Repositories
{
    public class AgentSpendingPolicyRepository(ApplicationDbContext dbContext) : IAgentSpendingPolicyRepository
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        public async Task AddAsync(AgentSpendingPolicy policy, CancellationToken cancellationToken)
        {
            await _dbContext.AgentSpendingPolicies.AddAsync(policy, cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<AgentSpendingPolicy?> GetCurrentWithAssetsAsync(Guid agentAccountId, CancellationToken cancellationToken)
        {
            return await _dbContext.AgentSpendingPolicies.AsTracking()
                .Include(x => x.Assets.Where(a => a.DeletedAt == null))
                .SingleOrDefaultAsync(
                    x => x.AgentAccountId == agentAccountId && x.DeletedAt == null,
                    cancellationToken
                );
        }
    }
}
