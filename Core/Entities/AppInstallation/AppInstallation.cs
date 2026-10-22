using static Core.Helpers.ApplicationEnum;

namespace Core.Entities.AppInstallation
{
    public class AppInstallation
    {
        public Guid Id { get; set; }

        public AppPlatformType Platform { get; set; }

        public AppInstallationStatus Status { get; set; } = AppInstallationStatus.Active;

        public string? AppVersion { get; set; }

        public string? OsVersion { get; set; }

        public string? DeviceModel { get; set; }

        public string? FirstSeenIp { get; set; }

        public string? LastSeenIp { get; set; }

        public string? FirstSeenCountry { get; set; }

        public string? LastSeenCountry { get; set; }

        public DateTimeOffset? LastSeenAt { get; set; }

        public DateTimeOffset DateCreated { get; set; }

        public DateTimeOffset? DateUpdated { get; set; }

        public DateTimeOffset? DeletedAt { get; set; }

        public ICollection<AgentAccount> AgentAccounts { get; set; } = [];
    }
}
