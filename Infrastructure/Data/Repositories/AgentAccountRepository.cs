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

        public async Task<AgentAccountRevocationContextResponseDto?> GetRevocationContextAsync(
            AgentAccountRevocationContextRequestDto request, CancellationToken cancellationToken)
        {
            // The cached delegated address also identifies the historical registration after a lost response.
            return await _dbContext.AgentSigningAuthorities.AsNoTracking()
                .Where(s => s.DelegatedAuthorityIdentifier == request.DelegatedAuthorityIdentifier &&
                    s.DeletedAt == null && s.AgentAccount.AppInstallationId == request.AppInstallationId &&
                    s.AgentAccount.ChainId == request.ChainId && s.AgentAccount.PublicAddress == request.PublicAddress &&
                    (!request.AgentAccountId.HasValue || s.AgentAccountId == request.AgentAccountId.Value) &&
                    (!request.SigningAuthorityId.HasValue || s.Id == request.SigningAuthorityId.Value) &&
                    ((s.Status == AgentSigningAuthorityStatus.Active && s.RevokedAt == null &&
                      s.AgentAccount.Status == AgentAccountStatus.Active && s.AgentAccount.DeletedAt == null) ||
                     (s.Status == AgentSigningAuthorityStatus.Revoked && s.RevokedAt != null &&
                      s.AgentAccount.Status == AgentAccountStatus.Revoked && s.AgentAccount.DeletedAt != null)))
                .Select(s => new AgentAccountRevocationContextResponseDto
                {
                    AgentAccountId = s.AgentAccountId, SigningAuthorityId = s.Id,
                    AppInstallationId = s.AgentAccount.AppInstallationId,
                    ChainId = s.AgentAccount.ChainId, PublicAddress = s.AgentAccount.PublicAddress,
                    DelegatedAuthorityIdentifier = s.DelegatedAuthorityIdentifier,
                    SigningAuthorityStatus = s.Status,
                    IsRevoked = s.AgentAccount.Status == AgentAccountStatus.Revoked
                }).SingleOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> RevokeAsync(CompleteAgentAccountRevocationRequestDto request,
            DateTimeOffset now, CancellationToken cancellationToken)
        {
            if (!request.AgentAccountId.HasValue || request.AgentAccountId == Guid.Empty ||
                !request.SigningAuthorityId.HasValue || request.SigningAuthorityId == Guid.Empty)
                return false;

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable, cancellationToken);
            var context = await GetRevocationContextAsync(request, cancellationToken);
            if (context == null) return false;
            if (context.IsRevoked)
            {
                await transaction.CommitAsync(cancellationToken);
                return true;
            }

            var account = await _dbContext.AgentAccounts.SingleAsync(x => x.Id == context.AgentAccountId, cancellationToken);
            var authority = await _dbContext.AgentSigningAuthorities.SingleAsync(x => x.Id == context.SigningAuthorityId, cancellationToken);
            var policies = await _dbContext.AgentSpendingPolicies
                .Include(x => x.Assets.Where(a => a.DeletedAt == null))
                .Where(x => x.AgentAccountId == account.Id && x.DeletedAt == null).ToListAsync(cancellationToken);
            var connections = await _dbContext.AgentPlatformConnections
                .Where(x => x.AgentAccountId == account.Id).ToListAsync(cancellationToken);

            account.Status = AgentAccountStatus.Revoked;
            account.DeletedAt = now;
            account.DateUpdated = now;
            authority.Status = AgentSigningAuthorityStatus.Revoked;
            authority.RevokedAt = now;
            authority.DateUpdated = now;
            foreach (var policy in policies)
            {
                policy.DeletedAt = now;
                policy.DateUpdated = now;
                foreach (var asset in policy.Assets)
                {
                    asset.DeletedAt = now;
                    asset.DateUpdated = now;
                }
            }
            foreach (var connection in connections)
                connection.Status = AgentPlatformConnectionStatus.Revoked;

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
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
