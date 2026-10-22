namespace Infrastructure.Data.Repositories
{
    public class BusinessRepository(ApplicationDbContext dbContext) : IBusinessRepository
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        public async Task<Business?> GetActiveByPublicIdAsync(string publicId, CancellationToken cancellationToken)
        {
            return await _dbContext.Businesses.AsNoTracking().FirstOrDefaultAsync(
                    x => x.PublicId == publicId && x.IsActive && x.DeletedAt == null, cancellationToken);
        }
    }
}
