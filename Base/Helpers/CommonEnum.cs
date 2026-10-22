namespace Base.Helpers
{
    public static class CommonEnum
    {
        public enum DateTimeAdd
        {
            Hours,
            Seconds,
            Minutes,
            Days,
            Months,
            Years
        }
    }

    public static class ContentType
    {
        public static string JSON { get { return "application/json"; } }
    }

    public static class KeyValut
    {
        public static string BASEPATH { get { return "AzureKeyVault:Url"; } }
    }

    public static class ConfigPath
    {
        public static string TOKEN_ISSUER { get { return "JwtIssuerOptions:Issuer"; } }
        public static string AUTHCONFIG_TENANTID { get { return "OAuthConfig:TenantId"; } }
        public static string AUTHCONFIG_CLIENTID { get { return "OAuthConfig:ClientId"; } }
        public static string OAUTHCONFIG_SECRET { get { return "OAuthConfig:ClientSecret"; } }
        // For the Swagger
        public static string SWAGGER_CONFIG { get { return "SwaggerConfig"; } }
    }
}
