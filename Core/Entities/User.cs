namespace Core.Entities
{
    public class User : IdentityUser
    {
        public string? DisplayName { get; set; }
        public string? UserSetting { get; set; }
        public bool IsDisabled { get; set; }
        public UserLevel UserLevel { get; set; } = UserLevel.User;
        public DateTimeOffset? LastLoginAt { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateUpdated { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public Business? Business { get; set; }
    }
}
