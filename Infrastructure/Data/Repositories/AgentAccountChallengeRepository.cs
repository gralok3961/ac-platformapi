namespace Infrastructure.Data.Repositories
{
    public class AgentAccountChallengeRepository(
        ApplicationDbContext dbContext) : IAgentAccountChallengeRepository
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        public async Task AddAsync(AgentAccountChallenge challenge, CancellationToken cancellationToken)
        {
            await _dbContext.AgentAccountChallenges.AddAsync(challenge, cancellationToken);
        }

        public async Task<AgentAccountChallenge?> GetByChallengeIdAsync(string challengeId, CancellationToken cancellationToken)
        {
            return await _dbContext.AgentAccountChallenges.FirstOrDefaultAsync(x => x.ChallengeId == challengeId, cancellationToken);
        }
        
        public async Task<bool> TryConsumeAsync( string challengeId,
            Guid appInstallationId, string publicKey, string signature,
            DateTimeOffset now, CancellationToken cancellationToken)
        {
            var affectedRows = await _dbContext.AgentAccountChallenges
                .Where(x =>
                    x.ChallengeId == challengeId &&
                    x.AppInstallationId == appInstallationId &&
                    x.Purpose == AgentAccountChallengePurpose.RegisterAgentAccount &&
                    x.Status == AgentAccountChallengeStatus.Pending &&
                    x.ExpiresAt > now
                )
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(x => x.PublicKey, publicKey)
                        .SetProperty(x => x.Signature, signature)
                        .SetProperty(x => x.VerifiedAt, now)
                        .SetProperty(x => x.UsedAt, now)
                        .SetProperty(x => x.Status, AgentAccountChallengeStatus.Used)
                        .SetProperty(x => x.DateUpdated, now),
                    cancellationToken
                );

            return affectedRows == 1;
        }


        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}