using static Core.Helpers.ApplicationEnum;

namespace Core.Entities.Agents
{
    public class AgentAccountChallenge
    {
        public Guid Id { get; set; }
        public Guid AppInstallationId { get; set; }
        public string ChallengeId { get; set; } = string.Empty;
        public AgentAccountChallengePurpose Purpose { get; set; }
        public int ChainId { get; set; }
        public string PublicAddress { get; set; } = string.Empty;
        public string Challenge { get; set; } = string.Empty;
        public string Nonce { get; set; } = string.Empty;
        public AgentAccountChallengeStatus Status { get; set; }
        public string? PublicKey { get; set; }
        public string? Signature { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset? VerifiedAt { get; set; }
        public DateTimeOffset? UsedAt { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset? DateUpdated { get; set; }
    }
}