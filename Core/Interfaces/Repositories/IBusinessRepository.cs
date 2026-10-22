namespace Core.Interfaces.Repositories
{
    public interface IBusinessRepository
    {



        Task<Business?> GetActiveByPublicIdAsync(string publicId, CancellationToken cancellationToken);
    }
}
