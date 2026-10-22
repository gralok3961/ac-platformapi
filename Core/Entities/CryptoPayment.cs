namespace Core.Entities
{
    public class CryptoPayment
    {
        // CryptoPayments.PaymentAmount DECIMAL(28,8)
        public const int PaymentAmountScale = 8;
        public Guid Id { get; set; }

        public Guid BusinessId { get; set; }
        public Business Business { get; set; } = null!;

        public string? CheckoutDeviceId { get; set; }

        public string? UserId { get; set; }
        public User? User { get; set; }

        public string PaymentCode { get; set; } = string.Empty;
        public string? ExternalReference { get; set; }
        public string? PaymentSource { get; set; }

        public string CheckoutCurrency { get; set; } = string.Empty;
        public decimal CheckoutAmount { get; set; }
        public string? PaymentDescription { get; set; } = string.Empty;

        public string? BlockchainNetwork { get; set; }     // May be null while CREATED.
        public string? PaymentAsset { get; set; }
        public decimal? PaymentAmount { get; set; }
        public long? PaymentFeeNative { get; set; }
        public string? BlockchainTxHash { get; set; }
        public string? PayerAddress { get; set; }
        public string? ReceiverAddress { get; set; }        // May be null while CREATED.

        public string? PricingSnapshotJson { get; set; } // JSON snapshot (immutable)
        public decimal? FxRate { get; set; }
        public string? FxProvider { get; set; }
        public DateTimeOffset? FxCapturedAt { get; set; }

        public string? SettlementCurrency { get; set; }     // May be null while CREATED.
        public decimal? SettlementAmount { get; set; }

        public CryptoPaymentStatus Status { get; set; }
        public string? FailureReason { get; set; }

        public DateTimeOffset? RequestedAt { get; set; }    // May be null while CREATED.
        public DateTimeOffset? PaidAt { get; set; }
        public DateTimeOffset? RefundedAt { get; set; }

        // ===== Auditing (UTC, DateTimeOffset) =====
        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateUpdated { get; set; }
    }
}
