namespace Core.Interfaces.Services
{
    public interface IUcpPaymentService
    {
        Task<UcpCredentialResult> IssueCredentialAsync(IssueCredentialRequest request, string platformProfileUrl, CancellationToken cancellationToken);
        Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest request, CancellationToken cancellationToken);
    }
}
