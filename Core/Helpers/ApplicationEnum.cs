namespace Core.Helpers
{
    public class ApplicationEnum
    {

        public enum AppPlatformType : byte
        {
            Android = 1,
            iOS = 2
        }
        public enum AppInstallationStatus : byte
        {
            Active = 1,
            Suspended = 2,
            Revoked = 3
        }

        public enum AgentPlatformConnectionStatus : byte
        {
            Active = 1,
            Suspended = 2,
            Revoked = 3
        }
        public enum AgentAccountStatus : byte
        {
            Active = 1,
            Suspended = 2,
            Revoked = 3
        }
        public enum AgentSigningAuthorityStatus : byte
        {
            Provisioned = 1,
            Active = 2,
            Revoked = 3
        }
        public enum AgentAccountChallengePurpose : byte
        {
            RegisterAgentAccount = 1,
            RevokeAgentAccess = 2
        }
        public enum AgentAccountChallengeStatus : byte
        {
            Pending = 0,
            Used = 1,
            Expired = 2,
            Failed = 3
        }

        public enum CryptoPaymentSource
        {
            AGENT
        }


        public enum AgentSpendingPolicyState
        {
            NotConfigured = 1,
            Configured = 2,
            Expired = 3
        }
        public enum SqlServerErrorCode
        {
            DuplicateKey = 2601,
            UniqueConstraintViolation = 2627
        }
    }
}
