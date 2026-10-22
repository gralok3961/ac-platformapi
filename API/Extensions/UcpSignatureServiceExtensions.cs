using API.Auth.Ucp;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace API.Extensions
{
    public static class UcpSignatureServiceExtensions
    {
        public static IServiceCollection AddUcpSignatureServices(this IServiceCollection services, IConfiguration config)
        {
            // Validate on resolution so an unconfigured UCP endpoint fails closed without
            // preventing the existing JWT/API-key endpoints from starting.
            services.AddOptions<UcpSecurityOptions>()
                .Bind(config.GetSection(UcpSecurityOptions.SectionName))
                .Validate(options => options.IsValid(), "UcpSecurity configuration is missing or invalid.");
            services.TryAddSingleton(TimeProvider.System);
            services.AddSingleton<IUcpProfileProvider, UcpProfileProvider>();
            services.AddSingleton<UcpSignatureVerifier>();
            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, UcpSignatureAuthHandler>(
                    HashCrossProtocol.HashAuthSchemes.UcpSignature, _ => { });
            return services;
        }
    }
}
