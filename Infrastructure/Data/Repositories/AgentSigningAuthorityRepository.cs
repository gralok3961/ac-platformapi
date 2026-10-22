using Microsoft.Data.SqlClient;

namespace Infrastructure.Data.Repositories
{
    public class AgentSigningAuthorityRepository(ApplicationDbContext dbContext) : IAgentSigningAuthorityRepository
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        public async Task<AgentSigningAuthority?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _dbContext.AgentSigningAuthorities.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<AgentSigningAuthority> AddOrGetCurrentAsync(AgentSigningAuthority authority, CancellationToken cancellationToken)
        {
            await _dbContext.AgentSigningAuthorities.AddAsync(authority, cancellationToken);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);

                return authority;
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is SqlException sqlException &&
                      (sqlException.Number == (int)SqlServerErrorCode.DuplicateKey  ||
                       sqlException.Number == (int)SqlServerErrorCode.UniqueConstraintViolation))
            {
                // This insert lost a concurrent race.
                // Remove the failed entity from EF tracking before querying the winner.
                _dbContext.Entry(authority).State = EntityState.Detached;

                var currentAuthority =
                    await GetCurrentByAgentAccountIdAsync(authority.AgentAccountId, cancellationToken);

                if (currentAuthority == null)
                {
                    // The unique violation was not caused by another
                    // current authority for this Agent Account.
                    throw;
                }

                return currentAuthority;
            }
        }

        public async Task<AgentSigningAuthority?> GetCurrentByAgentAccountIdAsync(Guid agentAccountId, CancellationToken cancellationToken)
        {
            return await _dbContext.AgentSigningAuthorities
                .FirstOrDefaultAsync(
                    x =>
                        x.AgentAccountId == agentAccountId &&
                        x.DeletedAt == null &&
                        (
                            x.Status == AgentSigningAuthorityStatus.Provisioned ||
                            x.Status == AgentSigningAuthorityStatus.Active
                        ),
                    cancellationToken
                );
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
