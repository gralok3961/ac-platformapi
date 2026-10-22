namespace Core.Entities.Security
{
    public class RequestNonce
    {
        public Guid Id { get; set; }

        public string Scope { get; set; } = string.Empty;

        public string Nonce { get; set; } = string.Empty;

        public DateTimeOffset DateCreated { get; set; }

        public DateTimeOffset ExpiresAt { get; set; }
    }
}