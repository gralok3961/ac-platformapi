using Microsoft.Data.SqlClient;
using Infrastructure.Helpers;

namespace Infrastructure.Data.Repositories
{
    public class AgentAccountRepository(ApplicationDbContext dbContext) : IAgentAccountRepository
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        public async Task<AgentAccount> AddOrGetExistingAsync(AgentAccount agentAccount, CancellationToken cancellationToken)
        {
            const int maxInsertAttempts = 3;

            for (var attempt = 1; ; attempt++)
            {
                await _dbContext.AgentAccounts.AddAsync(agentAccount, cancellationToken);

                try
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    return agentAccount;
                }
                catch (DbUpdateException ex) when (IsUniqueIndexViolation(ex, "UX_AgentAccounts_PublicId"))
                {
                    // Only retry a PublicId collision on this not-yet-persisted account.
                    _dbContext.Entry(agentAccount).State = EntityState.Detached;
                    if (attempt >= maxInsertAttempts)
                    {
                        throw;
                    }

                    agentAccount.PublicId = AgentAccountPublicIdGenerator.Generate();
                }
                catch (DbUpdateException ex) when (IsUniqueIndexViolation(ex,
                    "UX_AgentAccounts_ChainId_PublicAddress", "UX_AgentAccounts_Network_PublicAddress"))
                {
                    // Preserve the existing chain/address registration race recovery.
                    _dbContext.Entry(agentAccount).State = EntityState.Detached;

                    var existing = await _dbContext.AgentAccounts.FirstOrDefaultAsync(
                        x => x.ChainId == agentAccount.ChainId &&
                             x.PublicAddress == agentAccount.PublicAddress &&
                             x.DeletedAt == null,
                        cancellationToken);

                    if (existing == null)
                    {
                        throw;
                    }

                    return existing;
                }
            }
        }

        private static bool IsUniqueIndexViolation(DbUpdateException exception, params string[] indexNames)
        {
            if (exception.InnerException is not SqlException sqlException)
            {
                return false;
            }

            foreach (SqlError error in sqlException.Errors)
            {
                if ((error.Number == (int)SqlServerErrorCode.DuplicateKey ||
                     error.Number == (int)SqlServerErrorCode.UniqueConstraintViolation) &&
                    indexNames.Any(name => error.Message.Contains($"'{name}'", StringComparison.Ordinal)))
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<AgentAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _dbContext.AgentAccounts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<AgentAccount?> GetByChainIdAndPublicAddressAsync(
            int chainId,
            string publicAddress,
            CancellationToken cancellationToken)
        {
            return await _dbContext.AgentAccounts.FirstOrDefaultAsync(
                x =>
                    x.ChainId == chainId &&
                    x.PublicAddress == publicAddress &&
                    x.DeletedAt == null,
                cancellationToken
            );
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<AgentAccount?> GetAccessStateSnapshotAsync(int chainId, string publicAddress, CancellationToken cancellationToken)
        {
            return await _dbContext.AgentAccounts.AsNoTracking().Where(x =>
                    x.ChainId == chainId &&
                    x.PublicAddress == publicAddress &&
                    x.DeletedAt == null)
                .Include(x => x.SigningAuthorities.Where(a =>
                    a.DeletedAt == null &&
                    (a.Status == AgentSigningAuthorityStatus.Provisioned ||
                     a.Status == AgentSigningAuthorityStatus.Active)))
                .Include(x => x.SpendingPolicies.Where(p =>
                    p.DeletedAt == null))
                .ThenInclude(p => p.Assets.Where(a =>
                    a.DeletedAt == null))
                .AsSingleQuery()
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<AgentAccount?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken)
        {
            return await _dbContext.AgentAccounts.AsNoTracking().FirstOrDefaultAsync(
                    x => x.PublicId == publicId &&
                         x.DeletedAt == null, cancellationToken);
        }
    }
}
