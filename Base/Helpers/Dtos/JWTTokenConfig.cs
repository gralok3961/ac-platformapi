using static Base.Helpers.CommonEnum;

namespace Base.Helpers.Dtos
{
    public class JWTTokenConfig
    {
        public string IssuerPath { get; set; }
        public string SecretKeyPath { get; set; }
        public int ExpiryValue { get; set; }
        public DateTimeAdd AddType { get; set; }
#nullable enable
        public string? AudiencePath { get; set; }
    }
}
