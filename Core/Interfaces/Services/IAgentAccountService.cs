namespace Core.Interfaces.Services
{
    public interface IAgentAccountService
    {
        //ACCOUNT
        Task<ResponseDto<IssueAgentAccountChallengeResponseDto>> IssueAgentAccountChallengeAsync(IssueAgentAccountChallengeRequestDto request, CancellationToken cancellationToken);
        Task<ResponseDto<AgentAccountChallengeDetailResponseDto>> GetAgentAccountChallengeByChallengeIdAsync(string challengeId, CancellationToken cancellationToken);
        Task<ResponseDto<ProvisionAgentAccountAccessResponseDto>> ProvisionAgentAccountAccessAsync(ProvisionAgentAccountAccessRequestDto request, CancellationToken cancellationToken);
        Task<ResponseDto<AgentAccountActivationContextResponseDto>> GetAgentAccountActivationContextAsync(Guid agentAccountId, CancellationToken cancellationToken);
        Task<ResponseDto<ActivateAgentSigningAuthorityResponseDto>> ActivateAgentSigningAuthorityAsync(ActivateAgentSigningAuthorityRequestDto request, CancellationToken cancellationToken);

        //TRANSACTION
        Task<ResponseDto<AgentTransactionExecutionContextResponseDto>> GetAgentTransactionExecutionContextAsync(Guid agentAccountId, CancellationToken cancellationToken);
        //State
        Task<ResponseDto<GetAgentAccountAccessStateResponseDto>> GetAgentAccountAccessStateAsync(GetAgentAccountAccessStateInternalRequestDto request, CancellationToken cancellationToken);
        //Policy
        Task<ResponseDto<GetAgentAccountAccessStateResponseDto>> SaveAgentSpendingPolicyAsync(SaveAgentSpendingPolicyInternalRequestDto request, CancellationToken cancellationToken);


        Task<bool> ConnectAgentPlatformDemoAsync(string agentId, CancellationToken cancellationToken);
    }
}