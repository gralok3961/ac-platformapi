namespace API.Extensions
{
    public static class PolicySchemeServiceExtentions
    {
        public static IServiceCollection AddPolicySchemeServices(this IServiceCollection services)
        {
            services.AddAuthorizationBuilder()
                .AddPolicy("UCP_BUSINESS", policy =>
                {
                    policy.AuthenticationSchemes.Add(API.Auth.Ucp.UcpBusinessApiKeyAuthHandler.SchemeName);
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(
                        HashCrossProtocol.CommonApiAuth.ClientTypeClaim,
                        HashCrossProtocol.CommonApiAuth.ClientTypeValue
                    );
                })
                .AddPolicy("UCP_AGENT", policy =>
                {
                    policy.AuthenticationSchemes.Add(HashCrossProtocol.HashAuthSchemes.UcpSignature);
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(HashClaimKeys.UCP_PLATFORM_PROFILE);
                })
                .AddPolicy("APP_CLIENT", policy =>
                {
                    policy.AuthenticationSchemes.Add(HashCrossProtocol.HashAuthSchemes.AppClientBearer);
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(HashClaimKeys.APP_INSTALLATION_ID);
                    policy.RequireClaim(HashClaimKeys.APP_CLIENT_TYPE, HashAppClientType.WALLET);
                })
                .AddPolicy("INTERNAL_API", policy =>
                {
                    policy.AuthenticationSchemes.Add(HashCrossProtocol.HashAuthSchemes.InternalApiKey);
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(
                        HashCrossProtocol.CommonApiAuth.ClientTypeClaim,
                        HashCrossProtocol.CommonApiAuth.ClientTypeValue
                    );
                });
            return services;
        }
    }
}
