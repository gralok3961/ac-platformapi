namespace Infrastructure.Data.Repositories
{
    public class AgentSpendingPolicyAssetRepository(ApplicationDbContext dbContext) : IAgentSpendingPolicyAssetRepository
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        public async Task AddAsync(AgentSpendingPolicyAsset asset, CancellationToken cancellationToken)
        {
            await _dbContext.AgentSpendingPolicyAssets.AddAsync(asset, cancellationToken);
        }


    }
}
