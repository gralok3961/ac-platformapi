using Base.Helpers.Dtos;
using System.Net;

namespace Infrastructure.Helpers
{
    public partial class AgentAccountExecutionHelper
    {
        [System.Text.RegularExpressions.GeneratedRegex(
            @"^[A-Za-z0-9_-]{16,128}$")]
        private static partial System.Text.RegularExpressions.Regex ValidNonceRegex();

        [System.Text.RegularExpressions.GeneratedRegex(
            @"^(?:0|[1-9][0-9]{0,19})(?:\.[0-9]{1,18})?$")]
        private static partial System.Text.RegularExpressions.Regex ValidSpendingLimitRegex();

        public static ResponseDto<ProvisionAgentAccountAccessResponseDto>? ValidateProvisionRequest(
            ProvisionAgentAccountAccessRequestDto request)
        {
            if (request.AppInstallationId == Guid.Empty)
            {
                return new ResponseDto<ProvisionAgentAccountAccessResponseDto>(
                    HttpStatusCode.BadRequest, "Invalid app installation.");
            }

            return null;
        }

        public static ResponseDto<ProvisionAgentAccountAccessResponseDto>? ValidateProvisionChallenge(
            AgentAccountChallenge challenge, ProvisionAgentAccountAccessRequestDto request)
        {
            if (challenge.AppInstallationId != request.AppInstallationId)
            {
                return new ResponseDto<ProvisionAgentAccountAccessResponseDto>(
                    HttpStatusCode.Forbidden,
                    "Challenge does not belong to this app installation.");
            }

            if (challenge.Purpose != AgentAccountChallengePurpose.RegisterAgentAccount)
            {
                return new ResponseDto<ProvisionAgentAccountAccessResponseDto>(
                    HttpStatusCode.BadRequest, "Invalid challenge purpose.");
            }

            if (challenge.Status != AgentAccountChallengeStatus.Pending)
            {
                return new ResponseDto<ProvisionAgentAccountAccessResponseDto>(
                    HttpStatusCode.BadRequest, "Challenge is not pending.");
            }

            if (challenge.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                return new ResponseDto<ProvisionAgentAccountAccessResponseDto>(
                    HttpStatusCode.BadRequest, "Challenge has expired.");
            }
            return null;
        }
        public static ResponseDto<GetAgentAccountAccessStateResponseDto>? ValidateSaveSpendingPolicyRequest(
        SaveAgentSpendingPolicyInternalRequestDto? request)
        {
            static ResponseDto<GetAgentAccountAccessStateResponseDto> BadRequest(string message)
                => new(HttpStatusCode.BadRequest, message);

            // 1. Request validation
            if (request == null ||
                request.AppInstallationId == Guid.Empty ||
                request.ChainId <= 0 ||
                string.IsNullOrWhiteSpace(request.PublicAddress))
            {
                return BadRequest("Invalid spending policy request.");
            }

            // 2. Timestamp validation
            var now = DateTimeOffset.UtcNow;
            var timestamp = now.ToUnixTimeSeconds();

            if (request.Timestamp < timestamp - 120 ||
                request.Timestamp > timestamp + 120)
            {
                return BadRequest("Invalid or expired request timestamp.");
            }

            // 3. Nonce validation
            if (string.IsNullOrWhiteSpace(request.Nonce) ||
                !ValidNonceRegex().IsMatch(request.Nonce))
            {
                return BadRequest("Invalid request nonce.");
            }

            // 4. Policy validation
            if (request.Policy == null ||
                request.Policy.Assets == null)
            {
                return BadRequest("Invalid spending policy.");
            }

            // Policy CREATE / UPDATE requires a future expiration.
            // Policy removal (empty assets) is allowed even if expired.
            if (request.Policy.Assets.Count > 0 &&
                request.Policy.ExpiresAt <= now)
            {
                return BadRequest("Invalid policy expiration.");
            }

            // 5. Asset validation
            var assetKeys = new HashSet<(string, string?)>();

            foreach (var asset in request.Policy.Assets)
            {
                if (asset == null ||
                    string.IsNullOrWhiteSpace(asset.CurrencyCode) ||
                    asset.CurrencyCode.Length > 80 ||
                    asset.CurrencyCode != asset.CurrencyCode.Trim() ||
                    (asset.Issuer != null &&
                        (string.IsNullOrWhiteSpace(asset.Issuer) ||
                         asset.Issuer.Length > 200 ||
                         asset.Issuer != asset.Issuer.Trim())))
                {
                    return BadRequest("Invalid spending policy asset.");
                }

                // 6. Amount format and range validation
                if (string.IsNullOrWhiteSpace(asset.PerTransactionLimit) ||
                    !ValidSpendingLimitRegex().IsMatch(asset.PerTransactionLimit) ||
                    !decimal.TryParse(
                        asset.PerTransactionLimit,
                        System.Globalization.NumberStyles.AllowDecimalPoint,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out var amount) ||
                    amount <= 0)
                {
                    return BadRequest("Invalid per transaction limit.");
                }

                // 7. Decimal precision validation
                var normalizedInput = asset.PerTransactionLimit.Contains('.')
                    ? asset.PerTransactionLimit.TrimEnd('0').TrimEnd('.')
                    : asset.PerTransactionLimit;

                var normalizedAmount = amount.ToString(
                    "0.##################",
                    System.Globalization.CultureInfo.InvariantCulture
                );

                if (!string.Equals(normalizedInput, normalizedAmount, StringComparison.Ordinal))
                {
                    return BadRequest("Spending limit precision cannot be preserved.");
                }

                // 8. Duplicate asset validation
                if (!assetKeys.Add((asset.CurrencyCode, asset.Issuer)))
                {
                    return BadRequest("Duplicate spending policy asset.");
                }
            }

            return null;
        }

        public static ResponseDto<GetAgentAccountAccessStateResponseDto>? ValidateSupportedSpendingPolicyAssets(
        SaveAgentSpendingPolicyInternalRequestDto request,
        IReadOnlyList<DiscoverableTokenDto>? supportedTokens)
        {
            if (supportedTokens == null || supportedTokens.Count == 0)
            {
                return new ResponseDto<GetAgentAccountAccessStateResponseDto>(
                    HttpStatusCode.BadGateway, "Supported asset information is unavailable.");
            }

            foreach (var asset in request.Policy.Assets)
            {
                var isSupported = supportedTokens.Any(token =>
                {
                    // Native asset
                    if (asset.Issuer == null)
                    {
                        return token.IsNative &&
                            string.Equals(token.Symbol, asset.CurrencyCode, StringComparison.Ordinal);
                    }

                    // Issued asset (RLUSD, USDC, etc.)
                    return !token.IsNative &&
                        string.Equals(
                            token.CurrencyCode,
                            asset.CurrencyCode,
                            StringComparison.Ordinal) &&
                        string.Equals(
                            token.IssuerAddress,
                            asset.Issuer,
                            StringComparison.Ordinal);
                });

                if (!isSupported)
                {
                    return new ResponseDto<GetAgentAccountAccessStateResponseDto>(
                        HttpStatusCode.BadRequest, "Unsupported spending policy asset.");
                }
            }

            return null;
        }
    }
}