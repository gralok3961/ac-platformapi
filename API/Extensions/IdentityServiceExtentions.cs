using API.Auth.Ucp;

namespace API.Extensions
{
    public static class IdentityServiceExtentions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = HashCrossProtocol.HashAuthSchemes.AppClientBearer;
                options.DefaultChallengeScheme = HashCrossProtocol.HashAuthSchemes.AppClientBearer;
            })
            .AddJwtBearer(HashCrossProtocol.HashAuthSchemes.AppClientBearer, options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config[HashCrossConfigPath.AUTH_APPCLIENTKEY])),
                        ValidateIssuer = true,
                        ValidIssuer = config[ConfigPath.TOKEN_ISSUER],
                        ValidateAudience = true,
                        ValidAudience = config[HashCrossConfigPath.WALLET_APP_AUDIENCE],
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
            })
             // API KEY Authentication
            .AddScheme<AuthenticationSchemeOptions, InternalApiKeyAuthHandler>(
                HashCrossProtocol.HashAuthSchemes.InternalApiKey,
                options => { })

            // UCP Business API KEY Authentication
            .AddScheme<AuthenticationSchemeOptions, UcpBusinessApiKeyAuthHandler>(
                UcpBusinessApiKeyAuthHandler.SchemeName,
                options => { });

            services.AddUcpSignatureServices(config);

            return services;
        }
    }
}
