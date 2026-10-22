using Core.Entities.Security;

namespace Infrastructure.Data.Repositories
{
    public class RequestNonceRepository(
        ApplicationDbContext dbContext) : IRequestNonceRepository
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        public async Task AddAsync(RequestNonce nonce, CancellationToken cancellationToken)
        {
            await _dbContext.RequestNonces.AddAsync(nonce, cancellationToken);
        }
    }
}