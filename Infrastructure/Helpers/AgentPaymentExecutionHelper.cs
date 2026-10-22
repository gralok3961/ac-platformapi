
using System.Globalization;
using System.Text.Json.Nodes;

namespace Infrastructure.Helpers
{
    public static class AgentPaymentExecutionHelper
    {
        public static void ValidateIdempotencyKey(ProcessPaymentRequest request)
        {
            // Validate required fields before calculating the idempotency key.
            if (request == null || request.Items == null || request.Items.Any(item => item == null) ||
                request.Instrument?.Credential == null)
            {
                throw new UcpPaymentRequestException(
                    400, "invalid_payment_request", "Invalid payment request."
                );
            }

            var expectedKey = PaymentIdempotency.CreateKey(request);

            if (!string.Equals(request.IdempotencyKey, expectedKey, StringComparison.Ordinal))
            {
                throw new UcpPaymentRequestException(
                    409, "idempotency_conflict",
                    "The idempotency key does not match this payment operation."
                );
            }
        }

        public static BusinessPaymentTokenDto ResolveSelectedToken(ProcessPaymentInstrument instrument, BusinessPaymentContextDto paymentContext)
        {
            // Currently only XRPL execution is supported.
            if (!string.Equals(instrument.Network, "xrpl", StringComparison.OrdinalIgnoreCase))
                throw new UcpPaymentRequestException(400, "unsupported_network", "Unsupported payment network.");

            var matchingTokens = paymentContext.ActiveTokens
                .Where(x =>
                    string.Equals(x.NetworkAlias, instrument.Network, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(x.Symbol, instrument.Asset, StringComparison.Ordinal))
                .ToList();

            if (matchingTokens.Count != 1)
                throw new UcpPaymentRequestException(
                    403, "payment_asset_unavailable", "Payment asset is not available.");

            var selectedToken = matchingTokens[0];

            if (selectedToken.IsNative)
            {
                if (!string.IsNullOrWhiteSpace(selectedToken.CurrencyCode) ||
                    !string.IsNullOrWhiteSpace(selectedToken.IssuerAddress))
                    throw new UcpPaymentRequestException(
                        403, "payment_asset_invalid", "Invalid native asset configuration.");
            }
            else if (string.IsNullOrWhiteSpace(selectedToken.CurrencyCode) ||
                     string.IsNullOrWhiteSpace(selectedToken.IssuerAddress))
            {
                throw new UcpPaymentRequestException(
                    403, "payment_asset_invalid", "Invalid issued asset configuration.");
            }

            return selectedToken;
        }
        /// <summary>
        /// Converts a UCP checkout amount from minor units to major units.
        /// The merchant must provide the amount in ISO 4217 minor units,
        /// regardless of how the amount is stored internally.
        /// DecimalPlaces must come from the supported fiat currency configuration.
        /// Example: USD 300 with 2 decimal places becomes 3.00.
        /// </summary>
        public static decimal ToMajorUnits(long amount, int decimalPlaces)
        {
            if (amount <= 0 || decimalPlaces is < 0 or > 18)
                throw new UcpPaymentRequestException(400, "invalid_checkout_amount", "Invalid checkout amount.");

            decimal divisor = 1m;

            for (var i = 0; i < decimalPlaces; i++)
                divisor *= 10m;

            return amount / divisor;
        }

        public static CalculatedTokenPricingDto ResolveCalculatedToken(BusinessPaymentTokenDto selectedToken, CalculatedPricingSnapshotDto calculatedPricing)
        {
            var matchingTokens = calculatedPricing.Tokens.Where(x =>
                string.Equals(x.NetworkAlias, selectedToken.NetworkAlias, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.Symbol, selectedToken.Symbol, StringComparison.Ordinal) &&
                string.Equals(x.CurrencyCode, selectedToken.CurrencyCode, StringComparison.Ordinal) &&
                string.Equals(x.IssuerAddress, selectedToken.IssuerAddress, StringComparison.Ordinal))
                .Take(2)
                .ToList();

            if (matchingTokens.Count != 1)
                throw new UcpPaymentRequestException(
                    503, "payment_pricing_unavailable", "Selected asset pricing is unavailable.");

            return matchingTokens[0];
        }

        public static void ValidateSpendingLimit(AgentSpendingPolicy policy, BusinessPaymentTokenDto selectedToken, CalculatedTokenPricingDto calculatedToken)
        {
            var currencyCode = selectedToken.IsNative
                ? selectedToken.Symbol
                : selectedToken.CurrencyCode;

            var issuer = selectedToken.IsNative
                ? null
                : selectedToken.IssuerAddress;

            var rule = policy.Assets.SingleOrDefault(x =>
                x.DeletedAt == null &&
                string.Equals(x.CurrencyCode, currencyCode, StringComparison.Ordinal) &&
                string.Equals(x.Issuer, issuer, StringComparison.Ordinal));

            if (rule == null)
                throw new UcpPaymentRequestException(403, "spending_asset_denied", "Payment asset is not allowed by spending policy.");

            if (!decimal.TryParse(
                calculatedToken.RequiredAmount,
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out var requiredAmount) || requiredAmount <= 0)
                throw new UcpPaymentRequestException(503, "invalid_payment_amount", "Calculated payment amount is invalid.");

            if (requiredAmount > rule.PerTransactionLimit)
                throw new UcpPaymentRequestException(403, "spending_limit_exceeded", "Payment exceeds the spending limit.");
        }

        public static AgentSigningAuthority ValidateSigningAuthority(AgentSigningAuthority? authority)
        {
            if (authority == null ||
                authority.Status != AgentSigningAuthorityStatus.Active ||
                authority.RevokedAt != null ||
                authority.DeletedAt != null)
                throw new UcpPaymentRequestException(
                    403, "signing_authority_unavailable", "Agent signing authority is unavailable.");

            if (string.IsNullOrWhiteSpace(authority.ProviderKeyReference) ||
                string.IsNullOrWhiteSpace(authority.PublicKey) ||
                string.IsNullOrWhiteSpace(authority.DelegatedAuthorityIdentifier))
                throw new UcpPaymentRequestException(
                    503, "signing_authority_invalid", "Agent signing authority configuration is incomplete.");

            return authority;
        }

        public static AgentSpendingPolicy ValidateSpendingPolicy(AgentSpendingPolicy? policy)
        {
            if (policy == null ||
                (policy.ExpiresAt.HasValue && policy.ExpiresAt.Value <= DateTimeOffset.UtcNow))
                throw new UcpPaymentRequestException(
                    403, "spending_policy_unavailable",
                    "Agent spending policy is unavailable or expired.");

            return policy;
        }

        public static BusinessReceiveAddress ValidateReceiveAddress(BusinessReceiveAddress? receiveAddress, BusinessPaymentTokenDto selectedToken)
        {
            if (receiveAddress == null ||
                string.IsNullOrWhiteSpace(receiveAddress.PublicAddress) ||
                receiveAddress.BlockchainNetwork == null ||
                !string.Equals(
                    receiveAddress.BlockchainNetwork.Alias,
                    selectedToken.NetworkAlias,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new UcpPaymentRequestException(
                    403, "receiving_address_unavailable",
                    "Business receiving address is unavailable.");
            }

            return receiveAddress;
        }

        public static SupportedFiatCurrencyDto ValidateCheckoutCurrency(SupportedFiatCurrencyDto? currency, string checkoutCurrency)
        {
            if (currency == null ||
                !string.Equals(currency.CurrencyCode, checkoutCurrency, StringComparison.OrdinalIgnoreCase))
                throw new UcpPaymentRequestException(
                    503, "checkout_currency_unavailable", "Checkout currency information is unavailable.");

            return currency;
        }

        public static PricingSnapshotDto ValidatePricingSnapshot(PricingSnapshotDto? snapshot, string checkoutCurrency)
        {
            if (snapshot == null)
                throw new UcpPaymentRequestException(
                    503, "payment_pricing_unavailable", "Payment pricing is currently unavailable.");

            if (!string.Equals(snapshot.BaseCurrency, checkoutCurrency, StringComparison.OrdinalIgnoreCase))
                throw new UcpPaymentRequestException(
                    503, "payment_pricing_invalid", "Pricing currency mismatch.");

            return snapshot;
        }

        public static AgentPlatformConnection ValidatePlatformConnection(AgentPlatformConnection? connection)
        {
            if (connection == null ||
                connection.Status != AgentPlatformConnectionStatus.Active ||
                connection.ExpiresAt <= DateTimeOffset.UtcNow)
                throw new UcpPaymentRequestException(
                    403, "agent_authorization_denied", "Agent platform connection is unavailable.");

            return connection;
        }

        public static AgentAccount ValidateAgentAccount(AgentAccount? account)
        {
            if (account == null ||
                account.Status != AgentAccountStatus.Active ||
                account.DeletedAt != null)
                throw new UcpPaymentRequestException(
                    403, "agent_authorization_denied", "Agent account is unavailable.");

            return account;
        }

        public static ProcessPaymentCredential ValidateCredential(ProcessPaymentInstrument? instrument)
        {
            var credential = instrument?.Credential;

            if (credential == null || string.IsNullOrWhiteSpace(credential.Token))
                throw new UcpPaymentRequestException(
                    400, "invalid_payment_credential", "Payment credential is required.");

            return credential;
        }

        public static decimal ResolvePaymentAmount(CalculatedTokenPricingDto calculatedToken, BusinessPaymentTokenDto selectedToken)
        {
            if (selectedToken.Decimals is < 0 or > CryptoPayment.PaymentAmountScale)
                throw new UcpPaymentRequestException(
                    503, "invalid_asset_precision", "Unsupported asset precision.");

            if (!decimal.TryParse(
                calculatedToken.RequiredAmount,
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out var amount) || amount <= 0)
                throw new UcpPaymentRequestException(
                    503, "invalid_payment_amount", "Invalid payment amount.");

            if (decimal.Round(amount, selectedToken.Decimals) != amount ||
                amount >= 100_000_000_000_000_000_000m)
                throw new UcpPaymentRequestException(
                    503, "invalid_payment_precision", "Payment amount exceeds supported precision or range.");

            return amount;
        }

        public static string BuildAgentPricingSnapshotJson(CalculatedPricingSnapshotDto calculatedPricing, CalculatedTokenPricingDto selectedToken)
        {
            var snapshot = JsonNode.Parse(JsonHelper.ToJson(calculatedPricing))?.AsObject()
                ?? throw new InvalidOperationException("Invalid pricing snapshot.");

            snapshot["selectedAsset"] = new JsonObject
            {
                ["networkAlias"] = selectedToken.NetworkAlias,
                ["symbol"] = selectedToken.Symbol,
                ["currencyCode"] = selectedToken.CurrencyCode,
                ["issuerAddress"] = selectedToken.IssuerAddress
            };

            return snapshot.ToJsonString();
        }

        public static void ValidateExecutionResult(AgentPaymentExecutionResultDto result,
        Guid paymentId, Guid agentAccountId)
        {
            if (result == null ||
                result.RequestId != paymentId ||
                result.AgentAccountId != agentAccountId ||
                result.ResultCode is not ("SUCCESS" or "PENDING") ||
                string.IsNullOrWhiteSpace(result.TxHash) ||
                result.TxHash.Length != 64 ||
                !result.TxHash.All(Uri.IsHexDigit))
            {
                throw new UcpPaymentRequestException(
                    502,
                    "payment_execution_invalid_response", "Invalid agent payment execution response."
                );
            }
        }
    }
}