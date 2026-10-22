using Base.Services.HttpCallRequest;
using Microsoft.Extensions.Configuration;
using Base.Helpers.Dtos;
using System.Net;

namespace Infrastructure.Services
{
    public class TransactionHttpService(
        IHttpCallRequestService httpRequestService,
        IConfiguration configuration) : ITransactionHttpService
    {
        private readonly IHttpCallRequestService _httpRequestService = httpRequestService;
        private readonly IConfiguration _configuration = configuration;

        private Dictionary<string, string> GetInternalApiHeaders()
        {
            var internalApiKey = _configuration[HashCrossConfigPath.AUTH_INTERNALAPIKEY];

            if (string.IsNullOrWhiteSpace(internalApiKey))
            {
                throw new InvalidOperationException("Internal API key is not configured.");
            }

            return new Dictionary<string, string>
            {
                [HashCrossProtocol.HttpHeader.HASHAPIKEY] = internalApiKey
            };
        }


        public async Task<AgentPaymentExecutionResultDto> ExecuteAgentPaymentAsync(AgentPaymentExecutionRequestDto request)
        {
            var json = await _httpRequestService.SendPostRequestAsync(
                "agent-transactions/payment",
                request,
                noAuth: true,
                headers: GetInternalApiHeaders()
            );

            return json.FromJson<AgentPaymentExecutionResultDto>()
                ?? throw new InvalidOperationException("Invalid agent payment execution response.");
        }

    }
}
