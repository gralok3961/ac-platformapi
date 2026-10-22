using System.Net;
using Base.Helpers.Dtos;
using Base.Services.HttpCallRequest;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services
{
    public class SigningHttpService(IHttpCallRequestService httpRequestService, IConfiguration configuration) : ISigningHttpService
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

        public async Task<ResponseDto<ProvisionAgentSigningAuthorityResponseDto>> ProvisionAgentSigningAuthorityAsync(
                ProvisionAgentSigningAuthorityRequestDto request)
        {
            var json = await _httpRequestService.SendPostRequestAsync(
                "agent-signing-authority/provision", request, noAuth: true, headers: GetInternalApiHeaders()
            );

            var result = json.FromJson<ProvisionAgentSigningAuthorityResponseDto>();

            if (result == null)
            {
                return new ResponseDto<ProvisionAgentSigningAuthorityResponseDto>(
                    HttpStatusCode.BadGateway, "Invalid Signing API provisioning response.");
            }

            return new ResponseDto<ProvisionAgentSigningAuthorityResponseDto>(result);
        }
    }
}