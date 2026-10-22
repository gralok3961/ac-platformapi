using Base.Helpers.Dtos;
using Core.Entities.Security;
using Infrastructure.Helpers;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Net;
using static Base.Custom.BlockChains.BlockChainEnum;

namespace Infrastructure.Services
{
    public class AgentAccountService(IAgentAccountRepository agentAccountRepository,
    IAgentAccountChallengeRepository agentAccountChallengeRepository, IAgentSigningAuthorityRepository agentSigningAuthorityRepository,
    ISigningHttpService signingHttpService, IInfoFeedHttpService infoFeedHttpService, IAgentPlatformConnectionRepository agentPlatformConnectionRepository,
    IConfiguration configuration, IAgentSpendingPolicyRepository agentSpendingPolicyRepository, IAgentSpendingPolicyAssetRepository agentSpendingPolicyAssetRepository,
    IRequestNonceRepository requestNonceRepository
    ) : IAgentAccountService
    {
        private readonly IAgentPlatformConnectionRepository _agentPlatformConnectionRepository = agentPlatformConnectionRepository;
        private readonly IConfiguration _configuration = configuration;
        private readonly IAgentAccountRepository _agentAccountRepository = agentAccountRepository;
        private readonly IAgentAccountChallengeRepository _agentAccountChallengeRepository = agentAccountChallengeRepository;
        private readonly ISigningHttpService _signingHttpService = signingHttpService;
        private readonly IAgentSigningAuthorityRepository _agentSigningAuthorityRepository = agentSigningAuthorityRepository;
        private readonly IAgentSpendingPolicyRepository _agentSpendingPolicyRepository = agentSpendingPolicyRepository;
        private readonly IAgentSpendingPolicyAssetRepository _agentSpendingPolicyAssetRepository = agentSpendingPolicyAssetRepository;
        private readonly IRequestNonceRepository _requestNonceRepository = requestNonceRepository;
        private readonly IInfoFeedHttpService _infoFeedHttpService = infoFeedHttpService;

        public async Task<ResponseDto<IssueAgentAccountChallengeResponseDto>> IssueAgentAccountChallengeAsync(
                IssueAgentAccountChallengeRequestDto request, CancellationToken cancellationToken)
        {
            if (request.AppInstallationId == Guid.Empty)
            {
                return new ResponseDto<IssueAgentAccountChallengeResponseDto>(HttpStatusCode.BadRequest, "Invalid app installation.");
            }

            if (string.IsNullOrWhiteSpace(request.PublicAddress))
            {
                return new ResponseDto<IssueAgentAccountChallengeResponseDto>(HttpStatusCode.BadRequest, "Public address is required.");
            }

            var publicAddress = request.PublicAddress.Trim();
            var challengeId = Guid.NewGuid().ToString("N");
            var nonce = Base.Helpers.Utils.GenerateUrlSafeRandomKey(32);
            var expiresAt = DateTimeOffset.UtcNow.AddMinutes(5);

            var challenge =
                $"Hashtrion Agent Account Registration\n" +
                $"ChallengeId: {challengeId}\n" +
                $"AppInstallationId: {request.AppInstallationId}\n" +
                $"Address: {publicAddress}\n" +
                $"ChainId: {request.ChainId}\n" +
                $"Nonce: {nonce}\n" +
                $"ExpiresAt: {expiresAt:O}";

            var entity = new AgentAccountChallenge
            {
                AppInstallationId = request.AppInstallationId,
                ChallengeId = challengeId,
                ChainId = request.ChainId,
                PublicAddress = publicAddress,
                Challenge = challenge,
                Nonce = nonce,
                Purpose = AgentAccountChallengePurpose.RegisterAgentAccount,
                Status = AgentAccountChallengeStatus.Pending,
                ExpiresAt = expiresAt
            };

            await _agentAccountChallengeRepository.AddAsync(entity, cancellationToken);

            await _agentAccountChallengeRepository.SaveChangesAsync(cancellationToken);

            return new ResponseDto<IssueAgentAccountChallengeResponseDto>(
                new IssueAgentAccountChallengeResponseDto
                {
                    ChallengeId = challengeId,
                    Challenge = challenge,
                    ExpiresAt = expiresAt
                }
            );
        }

        public async Task<ResponseDto<AgentAccountChallengeDetailResponseDto>> GetAgentAccountChallengeByChallengeIdAsync(
                string challengeId, CancellationToken cancellationToken)
        {
            var cleanedChallengeId = challengeId.Trim();

            var challenge =
                await _agentAccountChallengeRepository.GetByChallengeIdAsync(cleanedChallengeId, cancellationToken);

            if (challenge == null)
            {
                return new ResponseDto<AgentAccountChallengeDetailResponseDto>(
                    HttpStatusCode.NotFound, "Agent account challenge not found."
                );
            }

            var dto = new AgentAccountChallengeDetailResponseDto
            {
                ChallengeId = challenge.ChallengeId,
                AppInstallationId = challenge.AppInstallationId,
                ChainId = challenge.ChainId,
                PublicAddress = challenge.PublicAddress,
                Challenge = challenge.Challenge,
                Purpose = challenge.Purpose,
                Status = challenge.Status,
                ExpiresAt = challenge.ExpiresAt
            };

            return new ResponseDto<AgentAccountChallengeDetailResponseDto>(dto);
        }
        // Provision Agent Account Access
        // 1. Validate request
        // 2. Fetch challenge by ChallengeId
        // 3. Validate challenge:
        //    - Pending
        //    - not expired
        //    - Purpose = RegisterAgentAccount
        //    - AppInstallationId matches
        // 4. Find existing AgentAccount by Chain/Address context
        // 5. Create AgentAccount if missing
        //    - Status = Active
        // 6. Mark challenge verified/used
        //    - PublicKey
        //    - Signature
        //    - VerifiedAt
        //    - UsedAt
        // 7. Save AgentAccount + challenge state
        // 8. Call Signing API to provision KMS authority
        // 9. Receive:
        //    - ProviderKeyReference
        //    - PublicKey
        //    - DelegatedAuthorityIdentifier (rKMS)
        // 10. Create AgentSigningAuthority
        //     - Status = Provisioned
        // 11. Save AgentSigningAuthority
        // 12. Return:
        //     - AgentAccountId
        //     - DelegatedAuthorityIdentifier

        public async Task<ResponseDto<ProvisionAgentAccountAccessResponseDto>> ProvisionAgentAccountAccessAsync(
            ProvisionAgentAccountAccessRequestDto request,
            CancellationToken cancellationToken)
        {
            var requestValidation = AgentAccountExecutionHelper.ValidateProvisionRequest(request);

            if (requestValidation != null) return requestValidation;

            var challengeId = request.ChallengeId.Trim();

            var challenge = await _agentAccountChallengeRepository.GetByChallengeIdAsync(challengeId, cancellationToken);

            if (challenge == null)
            {
                return new ResponseDto<ProvisionAgentAccountAccessResponseDto>(
                    HttpStatusCode.NotFound, "Agent account challenge not found."
                );
            }

            var challengeValidation = AgentAccountExecutionHelper.ValidateProvisionChallenge(challenge, request);

            if (challengeValidation != null) return challengeValidation;

            var now = DateTimeOffset.UtcNow;

            var agentAccount =
                await _agentAccountRepository.GetByChainIdAndPublicAddressAsync(challenge.ChainId, challenge.PublicAddress, cancellationToken);

            // Existing revoked Agent Account cannot be provisioned again.
            if (agentAccount?.Status == AgentAccountStatus.Revoked)
            {
                return new ResponseDto<ProvisionAgentAccountAccessResponseDto>(
                    HttpStatusCode.Forbidden, "Agent account has been revoked."
                );
            }

            // Atomically consume the challenge before creating/updating resources.
            var challengeConsumed =
                await _agentAccountChallengeRepository.TryConsumeAsync(
                    challengeId, request.AppInstallationId, request.PublicKey.Trim(),
                    request.Signature.Trim(), now, cancellationToken
                );

            if (!challengeConsumed)
            {
                return new ResponseDto<ProvisionAgentAccountAccessResponseDto>(
                    HttpStatusCode.Conflict, "Agent account challenge has already been used or is no longer valid."
                );
            }

            if (agentAccount == null)
            {
                agentAccount = await _agentAccountRepository.AddOrGetExistingAsync(
                        new AgentAccount
                        {
                            PublicId = AgentAccountPublicIdGenerator.Generate(),
                            AppInstallationId = request.AppInstallationId,
                            ChainId = challenge.ChainId,
                            PublicAddress = challenge.PublicAddress,
                            Status = AgentAccountStatus.Active
                        },
                        cancellationToken
                    );
            }

            // AddOrGetExistingAsync may have returned a row created
            // concurrently by another request, so validate again.
            if (agentAccount.Status == AgentAccountStatus.Revoked)
            {
                return new ResponseDto<ProvisionAgentAccountAccessResponseDto>(
                    HttpStatusCode.Forbidden, "Agent account has been revoked."
                );
            }

            if (agentAccount.AppInstallationId != request.AppInstallationId)
            {
                agentAccount.AppInstallationId = request.AppInstallationId;
                agentAccount.DateUpdated = now;
            }

            await _agentAccountRepository.SaveChangesAsync(cancellationToken);

            var currentAuthority =
                await _agentSigningAuthorityRepository.GetCurrentByAgentAccountIdAsync(
                    agentAccount.Id, cancellationToken
                );

            if (currentAuthority != null)
            {
                return new ResponseDto<ProvisionAgentAccountAccessResponseDto>(
                    new ProvisionAgentAccountAccessResponseDto
                    {
                        AgentAccountId = agentAccount.Id,
                        PublicId = agentAccount.PublicId,
                        DelegatedAuthorityIdentifier = currentAuthority.DelegatedAuthorityIdentifier
                    }
                );
            }

            var signingResult = await _signingHttpService.ProvisionAgentSigningAuthorityAsync(
                    new ProvisionAgentSigningAuthorityRequestDto
                    {
                        AgentAccountId = agentAccount.Id,
                        ChainId = agentAccount.ChainId
                    }
                );

            if (signingResult.Data == null)
            {
                return new ResponseDto<ProvisionAgentAccountAccessResponseDto>(
                    signingResult.StatusCode,
                    signingResult.Message
                );
            }

            var signingAuthority = signingResult.Data;

            currentAuthority = await _agentSigningAuthorityRepository.AddOrGetCurrentAsync(
                        new AgentSigningAuthority
                        {
                            AgentAccountId = agentAccount.Id,
                            Provider = signingAuthority.Provider,
                            ProviderKeyReference = signingAuthority.ProviderKeyReference,
                            PublicKey = signingAuthority.PublicKey,
                            DelegatedAuthorityIdentifier = signingAuthority.DelegatedAuthorityIdentifier,
                            Status = AgentSigningAuthorityStatus.Provisioned
                        },
                        cancellationToken
                    );

            return new ResponseDto<ProvisionAgentAccountAccessResponseDto>(
                new ProvisionAgentAccountAccessResponseDto
                {
                    AgentAccountId = agentAccount.Id,
                    PublicId = agentAccount.PublicId,
                    DelegatedAuthorityIdentifier = currentAuthority.DelegatedAuthorityIdentifier
                }
            );
        }

        public async Task<ResponseDto<AgentAccountActivationContextResponseDto>>
        GetAgentAccountActivationContextAsync(Guid agentAccountId, CancellationToken cancellationToken)
        {
            var agentAccount = await _agentAccountRepository.GetByIdAsync(agentAccountId, cancellationToken);

            if (agentAccount == null || agentAccount.DeletedAt != null)
            {
                return new ResponseDto<AgentAccountActivationContextResponseDto>(
                    HttpStatusCode.NotFound, "Agent account not found."
                );
            }
            //agent operation status which already active 
            if (agentAccount.Status != AgentAccountStatus.Active)
            {
                return new ResponseDto<AgentAccountActivationContextResponseDto>(
                    HttpStatusCode.Forbidden, "Agent account is not active."
                );
            }

            var signingAuthority =
                await _agentSigningAuthorityRepository.GetCurrentByAgentAccountIdAsync(agentAccount.Id, cancellationToken);

            if (signingAuthority == null)
            {
                return new ResponseDto<AgentAccountActivationContextResponseDto>(
                    HttpStatusCode.Conflict, "Agent signing authority is not provisioned."
                );
            }

            return new ResponseDto<AgentAccountActivationContextResponseDto>(
                new AgentAccountActivationContextResponseDto
                {
                    AgentAccountId = agentAccount.Id,
                    PublicId = agentAccount.PublicId,
                    AppInstallationId = agentAccount.AppInstallationId,
                    ChainId = agentAccount.ChainId,
                    PublicAddress = agentAccount.PublicAddress,
                    SigningAuthorityId = signingAuthority.Id,
                    DelegatedAuthorityIdentifier =
                        signingAuthority.DelegatedAuthorityIdentifier,
                    SigningAuthorityStatus = signingAuthority.Status
                }
            );
        }

        public async Task<ResponseDto<ActivateAgentSigningAuthorityResponseDto>> ActivateAgentSigningAuthorityAsync(
        ActivateAgentSigningAuthorityRequestDto request, CancellationToken cancellationToken)
        {
            var agentAccount = await _agentAccountRepository.GetByIdAsync(request.AgentAccountId, cancellationToken);

            if (agentAccount == null || agentAccount.DeletedAt != null)
            {
                return new ResponseDto<ActivateAgentSigningAuthorityResponseDto>(
                    HttpStatusCode.NotFound, "Agent account not found."
                );
            }

            if (agentAccount.Status != AgentAccountStatus.Active)
            {
                return new ResponseDto<ActivateAgentSigningAuthorityResponseDto>(
                    HttpStatusCode.Forbidden, "Agent account is not active."
                );
            }

            var signingAuthority = await _agentSigningAuthorityRepository.GetByIdAsync(request.SigningAuthorityId, cancellationToken);

            if (signingAuthority == null || signingAuthority.DeletedAt != null)
            {
                return new ResponseDto<ActivateAgentSigningAuthorityResponseDto>(
                    HttpStatusCode.NotFound, "Agent signing authority not found."
                );
            }

            if (signingAuthority.AgentAccountId != agentAccount.Id)
            {
                return new ResponseDto<ActivateAgentSigningAuthorityResponseDto>(
                    HttpStatusCode.BadRequest, "Agent signing authority does not belong to this Agent Account."
                );
            }

            // Idempotent success.
            if (signingAuthority.Status == AgentSigningAuthorityStatus.Active)
            {
                return new ResponseDto<ActivateAgentSigningAuthorityResponseDto>(
                    new ActivateAgentSigningAuthorityResponseDto
                    {
                        AgentAccountId = agentAccount.Id,
                        PublicId = agentAccount.PublicId,
                        SigningAuthorityId = signingAuthority.Id,
                        Status = signingAuthority.Status,
                        ActivatedAt = signingAuthority.ActivatedAt
                    }
                );
            }

            if (signingAuthority.Status == AgentSigningAuthorityStatus.Revoked)
            {
                return new ResponseDto<ActivateAgentSigningAuthorityResponseDto>(
                    HttpStatusCode.Forbidden, "Agent signing authority has been revoked."
                );
            }

            if (signingAuthority.Status != AgentSigningAuthorityStatus.Provisioned)
            {
                return new ResponseDto<ActivateAgentSigningAuthorityResponseDto>(
                    HttpStatusCode.Conflict, "Agent signing authority cannot be activated."
                );
            }

            var now = DateTimeOffset.UtcNow;

            signingAuthority.Status = AgentSigningAuthorityStatus.Active;
            signingAuthority.ActivatedAt = now;
            signingAuthority.DateUpdated = now;

            await _agentSigningAuthorityRepository.SaveChangesAsync(cancellationToken);

            return new ResponseDto<ActivateAgentSigningAuthorityResponseDto>(
                new ActivateAgentSigningAuthorityResponseDto
                {
                    AgentAccountId = agentAccount.Id,
                    PublicId = agentAccount.PublicId,
                    SigningAuthorityId = signingAuthority.Id,
                    Status = signingAuthority.Status,
                    ActivatedAt = signingAuthority.ActivatedAt
                }
            );
        }

        private static AgentSpendingPolicyState GetAgentSpendingPolicyState(AgentSpendingPolicy? policy)
        {
            if (policy == null)
                return AgentSpendingPolicyState.NotConfigured;

            if (policy.ExpiresAt.HasValue &&
                policy.ExpiresAt.Value <= DateTimeOffset.UtcNow)
                return AgentSpendingPolicyState.Expired;

            return AgentSpendingPolicyState.Configured;
        }

        public async Task<ResponseDto<AgentTransactionExecutionContextResponseDto>>
        GetAgentTransactionExecutionContextAsync(Guid agentAccountId, CancellationToken cancellationToken)
        {
            var agentAccount = await _agentAccountRepository.GetByIdAsync(agentAccountId, cancellationToken);

            if (agentAccount == null || agentAccount.DeletedAt != null)
            {
                return new ResponseDto<AgentTransactionExecutionContextResponseDto>(
                    HttpStatusCode.NotFound, "Agent Account not found."
                );
            }
            //Only Active
            if (agentAccount.Status != AgentAccountStatus.Active)
            {
                return new ResponseDto<AgentTransactionExecutionContextResponseDto>(
                    HttpStatusCode.Forbidden, "Agent Account is not active."
                );
            }

            var signingAuthority =
                await _agentSigningAuthorityRepository.GetCurrentByAgentAccountIdAsync(agentAccount.Id, cancellationToken);

            if (signingAuthority == null || signingAuthority.DeletedAt != null)
            {
                return new ResponseDto<AgentTransactionExecutionContextResponseDto>(
                    HttpStatusCode.NotFound, "Agent signing authority not found."
                );
            }
            //Only Active
            if (signingAuthority.Status != AgentSigningAuthorityStatus.Active)
            {
                return new ResponseDto<AgentTransactionExecutionContextResponseDto>(
                    HttpStatusCode.Conflict, "Agent signing authority is not active."
                );
            }

            return new ResponseDto<AgentTransactionExecutionContextResponseDto>(
                new AgentTransactionExecutionContextResponseDto
                {
                    AgentAccountId = agentAccount.Id,
                    ChainId = agentAccount.ChainId,
                    PublicAddress = agentAccount.PublicAddress,
                    SigningAuthorityId = signingAuthority.Id,
                    Provider = signingAuthority.Provider,
                    ProviderKeyReference =
                        signingAuthority.ProviderKeyReference,
                    PublicKey = signingAuthority.PublicKey,
                    DelegatedAuthorityIdentifier =
                        signingAuthority.DelegatedAuthorityIdentifier,
                    SigningAuthorityStatus =
                        signingAuthority.Status
                }
            );
        }

        public async Task<ResponseDto<GetAgentAccountAccessStateResponseDto>> GetAgentAccountAccessStateAsync(
        GetAgentAccountAccessStateInternalRequestDto request, CancellationToken cancellationToken)
        {
            // 1. Request validation
            if (request == null || request.ChainId <= 0 || request.AppInstallationId == Guid.Empty ||
                string.IsNullOrWhiteSpace(request.PublicAddress))
            {
                return new ResponseDto<GetAgentAccountAccessStateResponseDto>(
                    HttpStatusCode.BadRequest, "Invalid Agent Account access state request.");
            }
            // 2. Fetch Agent Account + Signing Authority + Policy + Assets
            var agentAccount = await _agentAccountRepository.GetAccessStateSnapshotAsync(request.ChainId, request.PublicAddress.Trim(), cancellationToken);

            if (agentAccount == null)
            {
                return new ResponseDto<GetAgentAccountAccessStateResponseDto>(HttpStatusCode.NotFound,
                 "Agent Account not found.");
            }
            // 3. Verify ownership
            if (agentAccount.AppInstallationId != request.AppInstallationId)
            {
                return new ResponseDto<GetAgentAccountAccessStateResponseDto>(HttpStatusCode.Forbidden,
                "Agent Account does not belong to this app installation.");
            }
            // 4. Verify public address
            if (!string.Equals(agentAccount.PublicAddress, request.PublicAddress.Trim(), StringComparison.Ordinal))
            {
                return new ResponseDto<GetAgentAccountAccessStateResponseDto>(HttpStatusCode.Forbidden,
                "Agent Account public address does not match.");
            }
            // 5. Verify Agent Account status
            if (agentAccount.Status != AgentAccountStatus.Active)
            {
                return new ResponseDto<GetAgentAccountAccessStateResponseDto>(HttpStatusCode.Forbidden,
                "Agent Account is not active.");
            }
            // 6. Current Signing Authority
            var signingAuthority = agentAccount.SigningAuthorities.SingleOrDefault();

            // 7. Current Spending Policy
            var spendingPolicy = agentAccount.SpendingPolicies.SingleOrDefault();

            // 8. Calculate Policy State
            var now = DateTimeOffset.UtcNow;

            var policyState = GetAgentSpendingPolicyState(spendingPolicy);

            // 9. Map Spending Policy
            AgentSpendingPolicyDto? policyDto = null;

            if (spendingPolicy != null)
            {
                policyDto = new AgentSpendingPolicyDto
                {
                    ExpiresAt = spendingPolicy.ExpiresAt,
                    Assets = [.. spendingPolicy.Assets
                        .Select(asset => new AgentSpendingPolicyAssetDto
                        {
                            CurrencyCode = asset.CurrencyCode,
                            Issuer = asset.Issuer,
                            PerTransactionLimit =
                                asset.PerTransactionLimit.ToString(
                                    "0.##################",
                                    CultureInfo.InvariantCulture)
                        })]
                };
            }

            // 10. Return Access State
            return new ResponseDto<GetAgentAccountAccessStateResponseDto>(
                new GetAgentAccountAccessStateResponseDto
                {
                    PublicId = agentAccount.PublicId,
                    SigningAuthorityStatus = signingAuthority?.Status,
                    PolicyStatus = policyState,
                    Policy = policyDto
                }
            );
        }

        public async Task<ResponseDto<GetAgentAccountAccessStateResponseDto>> SaveAgentSpendingPolicyAsync(
        SaveAgentSpendingPolicyInternalRequestDto request, CancellationToken cancellationToken)
        {
            // 1. Request validation
            var validationError = AgentAccountExecutionHelper.ValidateSaveSpendingPolicyRequest(request);

            if (validationError != null) return validationError;

            var network = (BlockChainId)request.ChainId switch
            {
                BlockChainId.XRPL => NetworkTypeAlias.XRPL,
                _ => null
            };

            if (network == null)
            {
                return new ResponseDto<GetAgentAccountAccessStateResponseDto>(
                    HttpStatusCode.BadRequest, "Unsupported Agent Account network.");
            }

            var publicAddress = request.PublicAddress.Trim();

            // 2. Fetch Agent Account
            var agentAccount = await _agentAccountRepository.GetAccessStateSnapshotAsync(
                request.ChainId, publicAddress, cancellationToken);

            if (agentAccount == null)
            {
                return new ResponseDto<GetAgentAccountAccessStateResponseDto>(
                    HttpStatusCode.NotFound, "Agent Account not found.");
            }

            // 3.Verify ownership
            if (agentAccount.AppInstallationId != request.AppInstallationId)
            {
                return new ResponseDto<GetAgentAccountAccessStateResponseDto>(
                    HttpStatusCode.Forbidden, "Agent Account does not belong to this app installation.");
            }

            // 4.Verify public address
            if (!string.Equals(agentAccount.PublicAddress, publicAddress, StringComparison.Ordinal))
            {
                return new ResponseDto<GetAgentAccountAccessStateResponseDto>(
                    HttpStatusCode.Forbidden, "Agent Account public address does not match.");
            }

            // 5. Verify Agent Account status
            if (agentAccount.Status != AgentAccountStatus.Active)
            {
                return new ResponseDto<GetAgentAccountAccessStateResponseDto>(
                    HttpStatusCode.Forbidden, "Agent Account is not active.");
            }

            // 6. Verify Signing Authority
            var signingAuthority = agentAccount.SigningAuthorities.SingleOrDefault();

            if (signingAuthority?.Status != AgentSigningAuthorityStatus.Active)
            {
                return new ResponseDto<GetAgentAccountAccessStateResponseDto>(
                    HttpStatusCode.Forbidden, "Agent signing authority is not active.");
            }

            // 6-1. Fetch supported XRPL assets
            // Supported asset validation is only required for CREATE / UPDATE.
            var isPolicyRemoval = request.Policy.Assets.Count == 0;

            if (!isPolicyRemoval)
            {
                var supportedTokensResult =
                    await _infoFeedHttpService.GetDiscoverableTokensByNetwork(network);

                if (supportedTokensResult.Data == null ||
                    supportedTokensResult.Data.Count == 0)
                {
                    return new ResponseDto<GetAgentAccountAccessStateResponseDto>(
                        HttpStatusCode.BadGateway,
                        "Supported XRPL asset information is unavailable.");
                }

                var assetValidation =
                    AgentAccountExecutionHelper.ValidateSupportedSpendingPolicyAssets(
                        request,
                        supportedTokensResult.Data);

                if (assetValidation != null) return assetValidation;
            }

            // 7. Load current Spending Policy
            var now = DateTimeOffset.UtcNow;

            var policy = await _agentSpendingPolicyRepository.GetCurrentWithAssetsAsync(agentAccount.Id, cancellationToken);

            // 8. Create or update Spending Policy
            if (isPolicyRemoval)
            {
                // Soft delete the entire Spending Policy.
                if (policy != null)
                {
                    policy.DeletedAt = now;
                    policy.DateUpdated = now;

                    // Soft delete all remaining active assets.
                    foreach (var asset in policy.Assets.Where(x => x.DeletedAt == null))
                    {
                        asset.DeletedAt = now;
                        asset.DateUpdated = now;
                    }
                }
            }
            else
            {
                // CREATE new policy if no active policy exists.
                if (policy == null)
                {
                    policy = new AgentSpendingPolicy
                    {
                        Id = Guid.NewGuid(),
                        AgentAccountId = agentAccount.Id,
                        ExpiresAt = request.Policy.ExpiresAt,
                        DateCreated = now
                    };

                    await _agentSpendingPolicyRepository.AddAsync(
                        policy,
                        cancellationToken
                    );
                }
                else
                {
                    // UPDATE existing policy.
                    policy.ExpiresAt = request.Policy.ExpiresAt;
                    policy.DateUpdated = now;
                }

                // 9. Synchronize Spending Policy Assets
                var existingAssets = policy.Assets.Where(x => x.DeletedAt == null).ToDictionary(x => (x.CurrencyCode, x.Issuer));

                var requestedKeys = new HashSet<(string CurrencyCode, string? Issuer)>();

                foreach (var requestedAsset in request.Policy.Assets)
                {
                    var key = (requestedAsset.CurrencyCode, requestedAsset.Issuer);

                    requestedKeys.Add(key);

                    var amount = decimal.Parse(
                        requestedAsset.PerTransactionLimit,
                        NumberStyles.AllowDecimalPoint,
                        CultureInfo.InvariantCulture
                    );

                    if (existingAssets.TryGetValue(key, out var existingAsset))
                    {
                        // UPDATE existing asset
                        if (existingAsset.PerTransactionLimit != amount)
                        {
                            existingAsset.PerTransactionLimit = amount;
                            existingAsset.DateUpdated = now;
                        }
                    }
                    else
                    {
                        // CREATE new asset
                        var newAsset = new AgentSpendingPolicyAsset
                        {
                            Id = Guid.NewGuid(),
                            AgentSpendingPolicyId = policy.Id,
                            CurrencyCode = requestedAsset.CurrencyCode,
                            Issuer = requestedAsset.Issuer,
                            PerTransactionLimit = amount,
                            DateCreated = now
                        };

                        await _agentSpendingPolicyAssetRepository.AddAsync(newAsset, cancellationToken);
                    }
                }

                // Soft delete assets removed from the policy
                foreach (var (key, existingAsset) in existingAssets)
                {
                    if (!requestedKeys.Contains(key))
                    {
                        existingAsset.DeletedAt = now;
                        existingAsset.DateUpdated = now;
                    }
                }
            }

            // 10. Register request nonce
            var requestNonce = new RequestNonce
            {
                Id = Guid.NewGuid(),
                Scope = "AgentSpendingPolicy.Save",
                Nonce = request.Nonce,
                DateCreated = now,
                ExpiresAt = now.AddMinutes(10)
            };

            await _requestNonceRepository.AddAsync(requestNonce, cancellationToken);

            // 11. Commit Nonce + Policy + Assets atomically
            try
            {
                await _agentSpendingPolicyRepository.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (
                ex.InnerException is SqlException sqlException &&
                (sqlException.Number == (int)SqlServerErrorCode.DuplicateKey ||
                sqlException.Number == (int)SqlServerErrorCode.UniqueConstraintViolation) &&
                sqlException.Message.Contains("UX_RequestNonces_Scope_Nonce", StringComparison.OrdinalIgnoreCase))
            {
                return new ResponseDto<GetAgentAccountAccessStateResponseDto>(
                    HttpStatusCode.Conflict, "Request nonce has already been used."
                );
            }
            // Return NotConfigured after the Spending Policy is removed.
            if (isPolicyRemoval)
            {
                return new ResponseDto<GetAgentAccountAccessStateResponseDto>(
                    new GetAgentAccountAccessStateResponseDto
                    {
                        PublicId = agentAccount.PublicId,
                        SigningAuthorityStatus = signingAuthority.Status,
                        PolicyStatus = AgentSpendingPolicyState.NotConfigured,
                        Policy = null
                    });
            }
            // 12. Build response from persisted tracked entities
            var savedAssets = policy.Assets.Where(x => x.DeletedAt == null).Select(x => new AgentSpendingPolicyAssetDto
            {
                CurrencyCode = x.CurrencyCode,
                Issuer = x.Issuer,
                PerTransactionLimit = x.PerTransactionLimit.ToString("0.##################", CultureInfo.InvariantCulture)
            }).ToList();

            return new ResponseDto<GetAgentAccountAccessStateResponseDto>(new GetAgentAccountAccessStateResponseDto
            {
                PublicId = agentAccount.PublicId,
                SigningAuthorityStatus = signingAuthority.Status,
                PolicyStatus = GetAgentSpendingPolicyState(policy),
                Policy = new AgentSpendingPolicyDto
                {
                    ExpiresAt = policy.ExpiresAt,
                    Assets = savedAssets
                }
            });
        }

        public async Task<bool> ConnectAgentPlatformDemoAsync(string agentId, CancellationToken cancellationToken)
        {
            var agentAccount = await _agentAccountRepository.GetByPublicIdAsync(agentId.Trim(), cancellationToken);

            if (agentAccount == null || agentAccount.Status != AgentAccountStatus.Active)
                return false;

            var trustedProfiles = _configuration.GetSection("UcpSecurity:TrustedPlatformProfiles").Get<string[]>();

            // The prototype is configured with one trusted Shopping Agent profile.
            if (trustedProfiles == null || trustedProfiles.Length != 1 ||
                string.IsNullOrWhiteSpace(trustedProfiles[0]))
                return false;

            return await _agentPlatformConnectionRepository.SaveDemoConnectionAsync(
                agentAccount.Id,
                trustedProfiles[0],
                DateTimeOffset.UtcNow.AddDays(30),
                cancellationToken);
        }
    }
}

