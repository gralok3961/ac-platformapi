namespace Core.Interfaces.Repositories
{
    public interface IBusinessPaymentContextRepository
    {


        Task<BusinessPaymentContextDto?> GetPaymentContextByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken);
    }
}
