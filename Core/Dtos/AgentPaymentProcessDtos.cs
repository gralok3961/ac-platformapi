using Newtonsoft.Json;

namespace Core.Dtos
{
    public class ProcessPaymentRequest
    {
        [Required]
        public string BusinessId { get; set; } = string.Empty;
        [Required]
        public string CheckoutId { get; set; } = string.Empty;
        [Required]
        public string CheckoutCurrency { get; set; } = string.Empty;
        [Range(0L, long.MaxValue)]
        public long CheckoutAmount { get; set; }
        [Required]
        public string PlatformProfile { get; set; } = string.Empty;
        [Required, MaxLength(300)]
        public string PaymentDescription { get; set; } = string.Empty;
        [Required, MinLength(1)]
        public List<ProcessPaymentItem> Items { get; set; } = [];
        [Required]
        public ProcessPaymentInstrument Instrument { get; set; } = null!;
        [Required]
        public string IdempotencyKey { get; set; } = string.Empty;
    }

    public class ProcessPaymentItem
    {
        [Required]
        public string Id { get; set; } = string.Empty;
        [Required]
        public string Title { get; set; } = string.Empty;
        [Range(1L, long.MaxValue)]
        public long Quantity { get; set; }
        [Range(0L, long.MaxValue)]
        public long UnitAmount { get; set; }
        [Range(0L, long.MaxValue)]
        public long LineTotal { get; set; }
    }

    public class ProcessPaymentInstrument
    {
        [Required]
        public string HandlerId { get; set; } = string.Empty;
        [Required]
        public string Id { get; set; } = string.Empty;
        [Required]
        public string Type { get; set; } = string.Empty;
        [Required]
        public string Network { get; set; } = string.Empty;
        [Required]
        public string Asset { get; set; } = string.Empty;
        [Required]
        public ProcessPaymentCredential Credential { get; set; } = null!;
    }

    public class ProcessPaymentCredential
    {
        [Required]
        public string Type { get; set; } = string.Empty;
        [Required]
        public string Token { get; set; } = string.Empty;
        public DateTimeOffset Expiry { get; set; }
    }

    public class ProcessPaymentResult
    {
        public Guid? PaymentId { get; set; }
        public string? ResultCode { get; set; }
        public string? TxHash { get; set; }

        public string? Code { get; set; }
        public string? Content { get; set; }
    }
    public class AgentPaymentExecutionRequestDto
    {
        [JsonProperty("paymentId")]
        public Guid PaymentId { get; set; }
        [JsonProperty("agentAccountId")]
        public Guid AgentAccountId { get; set; }
        [JsonProperty("providerKeyReference")]
        public string ProviderKeyReference { get; set; } = string.Empty;
        [JsonProperty("publicKey")]
        public string PublicKey { get; set; } = string.Empty;
    }

    public class AgentPaymentExecutionResultDto
    {
        [JsonProperty("requestId")]
        public Guid RequestId { get; set; }
        [JsonProperty("agentAccountId")]
        public Guid AgentAccountId { get; set; }
        [JsonProperty("network")]
        public string Network { get; set; } = string.Empty;
        [JsonProperty("txHash")]
        public string TxHash { get; set; } = string.Empty;
        [JsonProperty("ledgerIndex")]
        public uint? LedgerIndex { get; set; }
        [JsonProperty("closeTime")]
        public string? CloseTime { get; set; }
        [JsonProperty("deliveredAmount")]
        public string? DeliveredAmount { get; set; }
        [JsonProperty("feeCharged")]
        public string? FeeCharged { get; set; }
        [JsonProperty("resultCode")]
        public string ResultCode { get; set; } = string.Empty;
    }
}
