namespace Base.Custom.HashCross.Helpers
{
    public class HashCrossEnum
    {
        public static class HashCrossProtocol
        {
            public static class HttpHeader
            {
                public const string HASHAPIKEY = "x-hashtrion-internal-key"; //internal
            }
            public static class HashAuthSchemes
            {
                public const string AppClientBearer = "AppClientBearer";
                public const string InternalApiKey = "InternalApiKey";
                public const string UcpSignature = "UcpSignature";
            }
            public static class CommonApiAuth
            {
                public const string NameIdentifier = "common-api";
                public const string DisplayName = "Hashtrion API KEY";
                public const string ClientTypeClaim = "client_type";
                public const string ClientTypeValue = "common_api";
            }
        }

        public enum UserLevel
        {
            User = 0,
            Admin = 1,
            SuperAdmin = 2
        }

        public enum TokenStatus
        {
            Inactive = 0,
            Active = 1,
            Pending = 2
        }

        public enum CryptoPaymentStatus
        {
            CREATED = 0,
            PENDING = 1,
            PAID = 2,
            FAILED = 3,
            EXPIRED = 4,    //when nothing within specified time
            REFUNDED = 5,
            CANCELLED = 6,  // when user manually remove from ui
        }

        public enum AgentSigningProvider : byte
        {
            AzureKeyVault = 1,
            AwsKms = 2
        }
    }

    public static class HashKeyValut
    {
        public static string HASH_APPCLIENT { get { return "hashappclient"; } } //used for app client (wallet)
        public static string HASH_INTERNAL_API { get { return "hash-internal-apikey"; } } //internal apis
        public static string HASH_UCP_CREDENTIAL { get { return "hash-ucp-credential-secret"; } } //ucp-credential
    }

    public static class HashCrossConfigPath
    {
        public static string AUTH_APPCLIENTKEY { get { return "AuthAPIs:AppClientKey"; } }
        public static string AUTH_INTERNALAPIKEY { get { return "AuthAPIs:InternalAPIKey"; } }
        public static string AUTH_UCPCREDENTIAL { get { return "AuthAPIs:UcpCredential"; } }

        public static string INFOFEED_URL { get { return "ServiceUrl:InfoFeedService"; } }
        public static string TRANSACTION_URL { get { return "ServiceUrl:TransactionService"; } }
        public static string SIGNING_URL { get { return "ServiceUrl:SigningService"; } }
        public static string WALLET_APP_AUDIENCE { get { return "Audience:WalletApp"; } }
    }

    public static class HashClaimKeys
    {
        public const string UCP_PLATFORM_PROFILE = "UCP_PLATFORM_PROFILE";
        //For AppClient(wallet)
        public const string APP_INSTALLATION_ID = "appInstallationId";
        public const string APP_CLIENT_TYPE = "appClientType";
    }

    public static class HashAppClientType
    {
        public const string WALLET = "wallet";
    }
}
