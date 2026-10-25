//This is used by Transaction API ONLY to hide platform api URL to wallet
namespace API.Controllers
{
    [Authorize(Policy = "INTERNAL_API")]
    public class TransactionIntegrationController(IAgentAccountService agentAccountService) : BaseApiController
    {
        private readonly IAgentAccountService _agentAccountService = agentAccountService;
        [HttpPost("agent-account/revocation-context/a")]
        public async Task<IActionResult> GetAgentAccountRevocationContext(
            [FromBody] AgentAccountRevocationContextRequestDto request, CancellationToken cancellationToken)
        {
            var result = await _agentAccountService.GetAgentAccountRevocationContextAsync(request, cancellationToken);
            if (result.Data == null)
                return StatusCode((int)result.StatusCode, new ApiResponse(result.StatusCode, result.Message));
            return Ok(result.Data);
        }

        [HttpPost("agent-account/revoke/a")]
        public async Task<IActionResult> RevokeAgentAccount(
            [FromBody] CompleteAgentAccountRevocationRequestDto request, CancellationToken cancellationToken)
        {
            var result = await _agentAccountService.RevokeAgentAccountAsync(request, cancellationToken);
            if (result.Data == null)
                return StatusCode((int)result.StatusCode, new ApiResponse(result.StatusCode, result.Message));
            return Ok(result.Data);
        }

        //***** Agent related *************
        [HttpPost]
        [Route("agent-account/challenge/a")]
        public async Task<IActionResult> IssueAgentAccountChallenge([FromBody]
        IssueAgentAccountChallengeRequestDto request, CancellationToken cancellationToken)
        {
            var result = await _agentAccountService.IssueAgentAccountChallengeAsync(
                request,
                cancellationToken
            );

            if (result.Data == null)
                return StatusCode(
                    (int)result.StatusCode,
                    new ApiResponse(result.StatusCode, result.Message)
                );

            return Ok(result.Data);
        }

        [HttpGet]
        [Route("agent-account/challenge/{challengeId}")]
        public async Task<IActionResult> GetAgentAccountChallengeByChallengeId(string challengeId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(challengeId))
                return BadRequest(
                    new ApiResponse(HttpStatusCode.BadRequest, "Invalid challenge id.")
                );

            var result =
                await _agentAccountService.GetAgentAccountChallengeByChallengeIdAsync(challengeId, cancellationToken);

            if (result.Data == null)
                return StatusCode(
                    (int)result.StatusCode, new ApiResponse(result.StatusCode, result.Message)
                );

            return Ok(result.Data);
        }

        [HttpPost]
        [Route("agent-account/provision/a")]
        public async Task<IActionResult> ProvisionAgentAccountAccess([FromBody] ProvisionAgentAccountAccessRequestDto request,
            CancellationToken cancellationToken)
        {
            var result = await _agentAccountService.ProvisionAgentAccountAccessAsync(
                request, cancellationToken
            );

            if (result.Data == null)
                return StatusCode(
                    (int)result.StatusCode, new ApiResponse(result.StatusCode, result.Message)
                );

            return Ok(result.Data);
        }

        [HttpGet]
        [Route("agent-account/activation-context/{agentAccountId:guid}")]
        public async Task<IActionResult> GetAgentAccountActivationContext(
        Guid agentAccountId, CancellationToken cancellationToken)
        {
            var result = await _agentAccountService.GetAgentAccountActivationContextAsync(agentAccountId, cancellationToken);

            if (result.Data == null)
            {
                return StatusCode((int)result.StatusCode, new ApiResponse(result.StatusCode, result.Message));
            }

            return Ok(result.Data);
        }

        [HttpPost]
        [Route("agent-account/signing-authority/activate/a")]
        public async Task<IActionResult> ActivateAgentSigningAuthority([FromBody] ActivateAgentSigningAuthorityRequestDto request,
        CancellationToken cancellationToken)
        {
            var result = await _agentAccountService.ActivateAgentSigningAuthorityAsync(request, cancellationToken);

            if (result.Data == null)
            {
                return StatusCode((int)result.StatusCode, new ApiResponse(result.StatusCode, result.Message));
            }

            return Ok(result.Data);
        }

        [HttpGet]
        [Route("agent-account/transaction-execution-context/{agentAccountId:guid}")]
        public async Task<IActionResult> GetAgentTransactionExecutionContext(Guid agentAccountId, CancellationToken cancellationToken)
        {
            var result = await _agentAccountService.GetAgentTransactionExecutionContextAsync(agentAccountId, cancellationToken);

            if (result.Data == null)
            {
                return StatusCode(
                    (int)result.StatusCode, new ApiResponse(result.StatusCode, result.Message)
                );
            }

            return Ok(result.Data);
        }

        [HttpPost]
        [Route("agent-account/access-state/a")]
        public async Task<IActionResult> GetAgentAccountAccessState([FromBody] GetAgentAccountAccessStateInternalRequestDto request,
        CancellationToken cancellationToken)
        {
            var result = await _agentAccountService.GetAgentAccountAccessStateAsync(request, cancellationToken);

            if (result.Data == null)
            {
                return StatusCode((int)result.StatusCode, new ApiResponse(result.StatusCode, result.Message));
            }

            return Ok(result.Data);
        }

        [HttpPost]
        [Route("agent-account/spending-policy/save/a")]
        public async Task<IActionResult> SaveAgentSpendingPolicy([FromBody] SaveAgentSpendingPolicyInternalRequestDto request,
        CancellationToken cancellationToken)
        {
            var result = await _agentAccountService.SaveAgentSpendingPolicyAsync(request, cancellationToken);

            if (result.Data == null)
            {
                return StatusCode((int)result.StatusCode, new ApiResponse(result.StatusCode, result.Message));
            }

            return Ok(result.Data);
        }
    }
}
