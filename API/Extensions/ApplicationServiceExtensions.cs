using Base.Services.HttpCallRequest;
namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {

            //Repo
            services.AddScoped<IBusinessRepository, BusinessRepository>();
            services.AddScoped<IReceiveAddressRepository, ReceiveAddressRepository>();
            services.AddScoped<ICryptoPaymentRepository, CryptoPaymentRepository>();
            services.AddScoped<IBusinessPaymentContextRepository, BusinessPaymentContextRepository>();
            services.AddScoped<IAgentAccountRepository, AgentAccountRepository>();
            services.AddScoped<IAgentPlatformConnectionRepository, AgentPlatformConnectionRepository>();
            services.AddScoped<IAgentSigningAuthorityRepository, AgentSigningAuthorityRepository>();
            services.AddScoped<IAgentAccountChallengeRepository, AgentAccountChallengeRepository>();
            services.AddScoped<IAgentSpendingPolicyRepository, AgentSpendingPolicyRepository>();
            services.AddScoped<IAgentSpendingPolicyAssetRepository, AgentSpendingPolicyAssetRepository>();
            services.AddScoped<IRequestNonceRepository, RequestNonceRepository>();

            //Service
            services.AddScoped<IInfoFeedHttpService, InfoFeedHttpService>();
            services.AddScoped<ITransactionHttpService>(serviceProvider =>
            {
                var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();

                var httpClient = serviceProvider.GetRequiredService<HttpClient>();

                var config = serviceProvider.GetRequiredService<IConfiguration>();

                var transactionUrl = config[HashCrossConfigPath.TRANSACTION_URL];

                if (string.IsNullOrWhiteSpace(transactionUrl))
                {
                    throw new InvalidOperationException("Transaction service URL is not configured.");
                }
                var httpRequestService = new HttpCallRequestService(httpContextAccessor, httpClient, transactionUrl);

                return new TransactionHttpService(httpRequestService, config);
            });

            services.AddScoped<ISigningHttpService>(serviceProvider =>
            {
                var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();

                var httpClient = serviceProvider.GetRequiredService<HttpClient>();

                var config = serviceProvider.GetRequiredService<IConfiguration>();

                var signingUrl = config[HashCrossConfigPath.SIGNING_URL];

                if (string.IsNullOrWhiteSpace(signingUrl))
                {
                    throw new InvalidOperationException("Signing service URL is not configured.");
                }

                var httpRequestService = new HttpCallRequestService(httpContextAccessor, httpClient, signingUrl);

                return new SigningHttpService(httpRequestService, config);
            });

            services.AddScoped<IPaymentCalculationService, PaymentCalculationService>();

            services.AddScoped<IAppSecurityService, AppSecurityService>();
            services.AddScoped<IAgentAccountService, AgentAccountService>();
            services.AddScoped<IUcpPaymentService, UcpPaymentService>();


            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(config.GetConnectionString("ApplicationDbConnection"), b => b.MigrationsAssembly("Infrastructure")), ServiceLifetime.Scoped);
            services.AddCors(opt =>
            {
                opt.AddPolicy("CorsPolicy", builder =>
                    {
                        builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().WithExposedHeaders("*");
                    });
            });
            services.AddScoped<IHttpCallRequestService>(serviceProvider =>
            {
                var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();

                var httpClient = serviceProvider.GetRequiredService<HttpClient>();

                var config = serviceProvider.GetRequiredService<IConfiguration>();

                return new HttpCallRequestService(httpContextAccessor, httpClient, config[HashCrossConfigPath.INFOFEED_URL]);
            });

            services.AddSingleton<ITokenService>(x => ActivatorUtilities.CreateInstance<TokenService>(x, config, new JWTTokenConfig
            {
                IssuerPath = ConfigPath.TOKEN_ISSUER,
                SecretKeyPath = HashCrossConfigPath.AUTH_APPCLIENTKEY,
                AddType = CommonEnum.DateTimeAdd.Hours,
                ExpiryValue = 8
            }));

            services.AddSingleton<IAppClientAuthService>(x => ActivatorUtilities.CreateInstance<AppClientAuthService>(x, new JWTTokenConfig
            {
                IssuerPath = ConfigPath.TOKEN_ISSUER,
                SecretKeyPath = HashCrossConfigPath.AUTH_APPCLIENTKEY,
                AudiencePath = HashCrossConfigPath.WALLET_APP_AUDIENCE,
                AddType = CommonEnum.DateTimeAdd.Days,
                ExpiryValue = 7 //Can be 30 days
            }));
            services.AddScoped<IUcpCredentialService, UcpCredentialService>();

            services.ValidateModelState();

            return services;
        }
    }
}
