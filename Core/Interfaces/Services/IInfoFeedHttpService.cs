
namespace Core.Interfaces.Services
{
    public interface IInfoFeedHttpService
    {
        Task<ResponseDto<PricingSnapshotDto>> GetPricingSnapshot(string currencyCode);
        Task<ResponseDto<IReadOnlyList<DiscoverableTokenDto>>> GetDiscoverableTokensByNetwork(string network);
        Task<ResponseDto<SupportedFiatCurrencyDto>> GetSupportedFiatCurrency(string currencyCode);
    }
}
