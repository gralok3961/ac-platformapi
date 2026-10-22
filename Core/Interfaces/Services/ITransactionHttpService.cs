
namespace Core.Interfaces.Services
{
    public interface ITransactionHttpService
    {

        Task<AgentPaymentExecutionResultDto> ExecuteAgentPaymentAsync(AgentPaymentExecutionRequestDto request);
    }
}
