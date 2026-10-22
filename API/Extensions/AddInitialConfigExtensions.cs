using ConfigPath = Base.Helpers.ConfigPath;

namespace API.Extensions
{
    public static class AddInitialConfigExtensions
    {
        public static IHostApplicationBuilder AddInitialConfig(this IHostApplicationBuilder builder, IConfiguration config)
        {
            var keyVaultUrl = config[KeyValut.BASEPATH];

            var client = new SecretClient(new Uri(keyVaultUrl),
                new ClientSecretCredential(
                    tenantId: config[ConfigPath.AUTHCONFIG_TENANTID],
                    clientId: config[ConfigPath.AUTHCONFIG_CLIENTID],
                    clientSecret: config[ConfigPath.OAUTHCONFIG_SECRET]));
            //AppClient
            var appclientsecret = client.GetSecret(HashKeyValut.HASH_APPCLIENT).Value;

            builder.Configuration[HashCrossConfigPath.AUTH_APPCLIENTKEY] = appclientsecret.Value;

            //Internal API
            var internalapiKey = client.GetSecret(HashKeyValut.HASH_INTERNAL_API).Value;

            builder.Configuration[HashCrossConfigPath.AUTH_INTERNALAPIKEY] = internalapiKey.Value;

            //Ucp Credential
            var ucpCredential = client.GetSecret(HashKeyValut.HASH_UCP_CREDENTIAL).Value;

            builder.Configuration[HashCrossConfigPath.AUTH_UCPCREDENTIAL] = ucpCredential.Value;   

            return builder;
        }
    }
}
