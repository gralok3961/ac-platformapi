namespace Core.Dtos
{
    public class UcpCredentialResult
    {
        public string Type { get; set; } = "hashtrion_authorization";
        public string Token { get; set; } = string.Empty;
        public DateTimeOffset Expiry { get; set; }
    }
    public class IssueCredentialRequest
    {
        [Required]
        public string AgentPublicId { get; set; } = string.Empty;
        [Required]
        public string BusinessId { get; set; } = string.Empty;
        [Required]
        public string CheckoutId { get; set; } = string.Empty;
    }

    // Only fixed, non-sensitive messages may cross this boundary.
    public sealed class UcpPaymentRequestException(int statusCode, string code, string content) : Exception(content)
    {
        public int StatusCode { get; } = statusCode;
        public string Code { get; } = code;
    }
}

