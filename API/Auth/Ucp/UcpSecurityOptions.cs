namespace API.Auth.Ucp
{
    public sealed class UcpSecurityOptions
    {
        public const string SectionName = "UcpSecurity";
        public const string SupportedVersion = "2026-08-25";
        //defaults
        public string PublicOrigin { get; set; } = string.Empty;
        public string[] TrustedPlatformProfiles { get; set; } = [];
        public int SignatureMaxAgeSeconds { get; set; } = 300;
        public int ClockSkewSeconds { get; set; } = 30;
        public int ProfileTimeoutSeconds { get; set; } = 5;
        public int ProfileCacheSeconds { get; set; } = 60;
        public int MaximumRequestBodyBytes { get; set; } = 128 * 1024;

        public bool IsValid()
        {
            return HasValidOrigin() && HasValidLimits() && HasValidTrustedProfiles();
        }

        private bool HasValidOrigin()
        {
            return Uri.TryCreate(PublicOrigin, UriKind.Absolute, out var origin)
                && origin.Scheme == Uri.UriSchemeHttps
                && origin.UserInfo.Length == 0 && origin.AbsolutePath == "/"
                && origin.Query.Length == 0 && origin.Fragment.Length == 0
                && !PublicOrigin.Any(c => char.IsWhiteSpace(c) || char.IsControl(c) || c == '\\');
        }

        private bool HasValidLimits()
        {
            return SignatureMaxAgeSeconds is >= 30 and <= 600
                && ClockSkewSeconds is >= 0 and <= 60
                && ProfileTimeoutSeconds is >= 1 and <= 30
                && ProfileCacheSeconds is >= 1 and <= 300
                && MaximumRequestBodyBytes is >= 1 and <= 1048576;
        }

        private bool HasValidTrustedProfiles()
        {
            if (TrustedPlatformProfiles == null || TrustedPlatformProfiles.Length is < 1 or > 32 ||
                TrustedPlatformProfiles.Distinct(StringComparer.Ordinal).Count() != TrustedPlatformProfiles.Length)
                return false;

            try
            {
                return TrustedPlatformProfiles.All(profile =>
                    !string.IsNullOrEmpty(profile) && profile.Length <= 512 &&
                    UcpAgentHeader.ParseProfile("profile=\"" + profile + "\"").AbsoluteUri == profile);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
