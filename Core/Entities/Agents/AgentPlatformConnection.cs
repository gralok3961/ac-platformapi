using static Core.Helpers.ApplicationEnum;

namespace Core.Entities.Agents
{
    public class AgentPlatformConnection
    {
        public Guid Id { get; set; }
        public Guid AgentAccountId { get; set; }
        public string PlatformProfileUrl { get; set; } = string.Empty;
        public AgentPlatformConnectionStatus Status { get; set; } = AgentPlatformConnectionStatus.Active;
        public DateTimeOffset ExpiresAt { get; set; }
        public AgentAccount AgentAccount { get; set; } = null!;
    }
}
