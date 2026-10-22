using System.Globalization;
using Infrastructure.Helpers;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Services.UCP
{

    // Preserve checkout, credential jti, and idempotency bindings to prevent duplicate payments.
    // Persist the transaction hash and PENDING state before XRPL submission.
    // Reconcile uncertain outcomes before treating a payment as final.

    public class UcpPaymentService(
        IAgentAccountRepository agentAccountRepository, IBusinessRepository businessRepository, ITransactionHttpService transactionHttpService,
        IUcpCredentialService ucpCredentialService, IBusinessPaymentContextRepository businessPaymentContextRepository, IReceiveAddressRepository receiveAddressRepository,
        IPaymentCalculationService paymentCalculationService, IAgentPlatformConnectionRepository agentPlatformConnectionRepository, IAgentSigningAuthorityRepository agentSigningAuthorityRepository,
        ICryptoPaymentRepository cryptoPaymentRepository, IInfoFeedHttpService infoFeedHttpService, IAgentSpendingPolicyRepository agentSpendingPolicyRepository) : IUcpPaymentService
    {
        private readonly ITransactionHttpService _transactionHttpService = transactionHttpService;
        private readonly IReceiveAddressRepository _receiveAddressRepository = receiveAddressRepository;
        private readonly IAgentSigningAuthorityRepository _agentSigningAuthorityRepository = agentSigningAuthorityRepository;
        private readonly IAgentSpendingPolicyRepository _agentSpendingPolicyRepository = agentSpendingPolicyRepository;
        private readonly IPaymentCalculationService _paymentCalculationService = paymentCalculationService;
        private readonly IInfoFeedHttpService _infoFeedHttpService = infoFeedHttpService;
        private readonly IBusinessPaymentContextRepository _businessPaymentContextRepository = businessPaymentContextRepository;
        private readonly ICryptoPaymentRepository _cryptoPaymentRepository = cryptoPaymentRepository;
        private readonly IBusinessRepository _businessRepository = businessRepository;
        private readonly IAgentAccountRepository _agentAccountRepository = agentAccountRepository;
        private readonly IUcpCredentialService _ucpCredentialService = ucpCredentialService;
        private readonly IAgentPlatformConnectionRepository _agentPlatformConnectionRepository = agentPlatformConnectionRepository;
        public async Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest request, CancellationToken cancellationToken)
        {
            // Validate the business and payment credential.
            var business = await _businessRepository.GetActiveByPublicIdAsync(request.BusinessId, cancellationToken);

            if (business == null) throw new UcpPaymentRequestException(403, "business_authorization_denied", "Business is unavailable.");

            var credential = AgentPaymentExecutionHelper.ValidateCredential(request.Instrument);

            AgentPaymentExecutionHelper.ValidateIdempotencyKey(request);

            var (agentPublicId, _) = _ucpCredentialService.Validate(credential.Token, business.PublicId, request.CheckoutId, request.PlatformProfile);

            // Verify the agent account and its authorized platform connection.
            var account = await _agentAccountRepository.GetByPublicIdAsync(agentPublicId, cancellationToken);

            var agentAccount = AgentPaymentExecutionHelper.ValidateAgentAccount(account);

            var connection = await _agentPlatformConnectionRepository.GetByAgentAccountIdAndPlatformProfileUrlAsync(
                agentAccount.Id, request.PlatformProfile, cancellationToken);

            AgentPaymentExecutionHelper.ValidatePlatformConnection(connection);

            // Prevent duplicate payments for the same checkout.
            var existingPayment = await _cryptoPaymentRepository.GetAgentPaymentByCheckoutIdAsync(business.Id, request.CheckoutId, cancellationToken);

            if (existingPayment != null) throw new UcpPaymentRequestException(409, "payment_already_exists", "A payment already exists for this checkout.");

            // Resolve payment options and calculate the required asset amount.
            var paymentContext = await _businessPaymentContextRepository.GetPaymentContextByBusinessIdAsync(business.Id, cancellationToken);

            if (paymentContext == null || paymentContext.ActiveNetworks.Count == 0 || paymentContext.ActiveTokens.Count == 0)
                throw new UcpPaymentRequestException(403, "payment_method_unavailable", "Business payment method is unavailable.");

            // Currently only XRPL execution is supported.
            var selectedToken = AgentPaymentExecutionHelper.ResolveSelectedToken(request.Instrument, paymentContext);

            var pricingResult = await _infoFeedHttpService.GetPricingSnapshot(request.CheckoutCurrency);

            var pricingSnapshot = AgentPaymentExecutionHelper.ValidatePricingSnapshot(pricingResult.Data, request.CheckoutCurrency);

            var currencyResult = await _infoFeedHttpService.GetSupportedFiatCurrency(request.CheckoutCurrency);

            var currency = AgentPaymentExecutionHelper.ValidateCheckoutCurrency(currencyResult.Data, request.CheckoutCurrency);

            var checkoutAmount = AgentPaymentExecutionHelper.ToMajorUnits(request.CheckoutAmount, currency.DecimalPlaces);

            var calculatedPricing = _paymentCalculationService.Calculate(paymentContext, checkoutAmount, pricingSnapshot);

            var calculatedToken = AgentPaymentExecutionHelper.ResolveCalculatedToken(selectedToken, calculatedPricing);

            if (calculatedToken == null)
                throw new UcpPaymentRequestException(503, "payment_pricing_unavailable", "Selected asset pricing is unavailable.");
            // Enforce the agent's spending policy before payment execution.
            var policy = await _agentSpendingPolicyRepository.GetCurrentWithAssetsAsync(agentAccount.Id, cancellationToken);

            var spendingPolicy = AgentPaymentExecutionHelper.ValidateSpendingPolicy(policy);

            AgentPaymentExecutionHelper.ValidateSpendingLimit(spendingPolicy, selectedToken, calculatedToken);

            // Verify signing authority and the business's receiving address.
            var authority = await _agentSigningAuthorityRepository.GetCurrentByAgentAccountIdAsync(agentAccount.Id, cancellationToken);

            AgentPaymentExecutionHelper.ValidateSigningAuthority(authority);

            var address = await _receiveAddressRepository.GetActiveByBusinessAndChainAsync(
                business.Id, agentAccount.ChainId, cancellationToken);

            var receiveAddress = AgentPaymentExecutionHelper.ValidateReceiveAddress(address, selectedToken);

            // Prepare the payment record with the validated pricing and transaction details.
            var now = DateTimeOffset.UtcNow;

            var paymentAmount = AgentPaymentExecutionHelper.ResolvePaymentAmount(calculatedToken, selectedToken);

            var payment = new CryptoPayment
            {
                Id = Guid.NewGuid(),
                BusinessId = business.Id,
                CheckoutDeviceId = null,
                PaymentSource = nameof(CryptoPaymentSource.AGENT),
                ExternalReference = request.CheckoutId,
                PaymentCode = ApplicationUtils.GeneratePaymentCode(business.BusinessCode),
                CheckoutCurrency = request.CheckoutCurrency,
                CheckoutAmount = checkoutAmount,
                PaymentDescription = request.PaymentDescription,
                PricingSnapshotJson = AgentPaymentExecutionHelper.BuildAgentPricingSnapshotJson(calculatedPricing, calculatedToken),
                Status = CryptoPaymentStatus.CREATED,
                DateCreated = now,
                DateUpdated = now,
                BlockchainNetwork = selectedToken.NetworkAlias,
                PaymentAsset = selectedToken.Symbol,
                PayerAddress = agentAccount.PublicAddress,
                ReceiverAddress = receiveAddress.PublicAddress,
                PaymentAmount = paymentAmount,
            };

            // Persist the payment and handle concurrent duplicate checkout attempts.
            try
            {
                await _cryptoPaymentRepository.CreateCryptoPayment(payment);
            }
            catch (DbUpdateException ex) when (
                ex.InnerException is SqlException sqlException &&
                (sqlException.Number == (int)SqlServerErrorCode.DuplicateKey ||
                sqlException.Number == (int)SqlServerErrorCode.UniqueConstraintViolation) &&
                sqlException.Message.Contains("UX_CryptoPayments_Agent_Business_Checkout", StringComparison.Ordinal))
            {
                throw new UcpPaymentRequestException(
                    409, "payment_already_exists", "A payment already exists for this checkout.");
            }

            // Execute the agent payment through Transaction API.
            var executionRequest = new AgentPaymentExecutionRequestDto
            {
                PaymentId = payment.Id,
                AgentAccountId = agentAccount.Id,
                ProviderKeyReference = authority!.ProviderKeyReference,
                PublicKey = authority.PublicKey
            };

            var executionResult = await _transactionHttpService.ExecuteAgentPaymentAsync(executionRequest);

            AgentPaymentExecutionHelper.ValidateExecutionResult(executionResult, payment.Id, agentAccount.Id);

            return new ProcessPaymentResult
            {
                PaymentId = payment.Id,
                ResultCode = executionResult.ResultCode,
                TxHash = executionResult.TxHash
            };
        }

        public async Task<UcpCredentialResult> IssueCredentialAsync(IssueCredentialRequest request, string platformProfileUrl, CancellationToken cancellationToken)
        {
            var agentAccount = await _agentAccountRepository.GetByPublicIdAsync(request.AgentPublicId, cancellationToken);

            if (agentAccount == null || agentAccount.Status != AgentAccountStatus.Active || agentAccount.DeletedAt != null)
            {
                throw new UnauthorizedAccessException("Agent account is unavailable.");
            }

            var connection = await _agentPlatformConnectionRepository.GetByAgentAccountIdAndPlatformProfileUrlAsync(
                 agentAccount.Id, platformProfileUrl, cancellationToken);

            if (connection == null || connection.Status != AgentPlatformConnectionStatus.Active ||
                connection.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                throw new UnauthorizedAccessException("Agent platform connection is unavailable.");
            }

            var business = await _businessRepository.GetActiveByPublicIdAsync(request.BusinessId, cancellationToken);

            if (business == null)
            {
                throw new UnauthorizedAccessException("Business is unavailable.");
            }

            //Checkout Id is not verifying here
            return _ucpCredentialService.Issue(agentAccount.PublicId, business.PublicId, request.CheckoutId, platformProfileUrl);
        }
    }
}
