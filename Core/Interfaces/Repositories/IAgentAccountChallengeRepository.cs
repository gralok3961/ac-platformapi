namespace Core.Interfaces.Repositories
{
    public interface IAgentAccountChallengeRepository
    {
        Task AddAsync(AgentAccountChallenge challenge, CancellationToken cancellationToken);

        Task<AgentAccountChallenge?> GetByChallengeIdAsync(string challengeId, CancellationToken cancellationToken);

        Task<bool> TryConsumeAsync(
            string challengeId,
            Guid appInstallationId,
            string publicKey,
            string signature,
            DateTimeOffset now,
            CancellationToken cancellationToken);

        Task SaveChangesAsync(CancellationToken cancellationToken);

    }
}