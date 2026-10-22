using static Core.Helpers.ApplicationEnum;

namespace Core.Interfaces.Repositories
{
    public interface IAgentSigningAuthorityRepository
    {
        Task<AgentSigningAuthority?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<AgentSigningAuthority?> GetCurrentByAgentAccountIdAsync(Guid agentAccountId, CancellationToken cancellationToken);
        Task<AgentSigningAuthority> AddOrGetCurrentAsync(AgentSigningAuthority authority, CancellationToken cancellationToken);

        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
