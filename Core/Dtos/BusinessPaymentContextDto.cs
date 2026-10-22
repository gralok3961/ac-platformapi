namespace Core.Dtos
{
    public sealed class BusinessPaymentContextDto
    {
        public List<string> ActiveNetworks { get; set; } = [];
        public List<BusinessPaymentTokenDto> ActiveTokens { get; set; } = [];
    }
}
