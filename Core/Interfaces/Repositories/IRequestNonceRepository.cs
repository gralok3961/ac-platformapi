using Core.Entities.Security;

namespace Core.Interfaces.Repositories
{
    public interface IRequestNonceRepository
    {
        Task AddAsync(RequestNonce nonce, CancellationToken cancellationToken);
    }
}