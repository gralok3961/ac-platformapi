using System.Net;
using Base.Helpers.Dtos;
using Base.Services.HttpCallRequest;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services
{
    public class InfoFeedHttpService(IHttpCallRequestService httpRequestService, IConfiguration configuration) : IInfoFeedHttpService
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
        public async Task<ResponseDto<PricingSnapshotDto>> GetPricingSnapshot(string currencyCode)
        {
            var json = await _httpRequestService.SendGetRequestAsync(
                "PlatformIntegration/pricing/snapshot",
                new { currencyCode }, noAuth: true, headers: GetInternalApiHeaders());

            var snapshot = json.FromJson<PricingSnapshotDto>();

            if (snapshot == null)
            {
                return new ResponseDto<PricingSnapshotDto>(HttpStatusCode.BadGateway, "Invalid pricing snapshot response");
            }

            return new ResponseDto<PricingSnapshotDto>(snapshot);
        }





        public async Task<ResponseDto<IReadOnlyList<DiscoverableTokenDto>>> GetDiscoverableTokensByNetwork(string network)
        {
            var json = await _httpRequestService.SendGetRequestAsync(
                $"PlatformIntegration/discoverable-tokens/network/{Uri.EscapeDataString(network)}",
                noAuth: true,
                headers: GetInternalApiHeaders());

            var tokens = json.FromJson<List<DiscoverableTokenDto>>();

            if (tokens == null)
            {
                return new ResponseDto<IReadOnlyList<DiscoverableTokenDto>>(
                    HttpStatusCode.BadGateway, "Invalid discoverable tokens by network response.");
            }

            return new ResponseDto<IReadOnlyList<DiscoverableTokenDto>>(tokens);
        }

        public async Task<ResponseDto<SupportedFiatCurrencyDto>> GetSupportedFiatCurrency(string currencyCode)
        {
            var json = await _httpRequestService.SendGetRequestAsync(
                $"PlatformIntegration/fiat-currencies/{Uri.EscapeDataString(currencyCode)}",
                noAuth: true,
                headers: GetInternalApiHeaders());

            var currency = json.FromJson<SupportedFiatCurrencyDto>();

            if (currency == null)
            {
                return new ResponseDto<SupportedFiatCurrencyDto>(
                    HttpStatusCode.BadGateway, "Invalid supported fiat currency response.");
            }

            return new ResponseDto<SupportedFiatCurrencyDto>(currency);
        }
    }
}
