using Core.Entities.Security;
using Infrastructure.Data.Configurations.Agents;
using Infrastructure.Data.Configurations.AppInstallations;

namespace Infrastructure.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : BaseIdentityDbContext<User>(options)
    {
        public DbSet<BlockchainNetwork> BlockchainNetworks { get; set; }
        public DbSet<BusinessAllowToken> BusinessAllowTokens { get; set; }
        public DbSet<DiscoverableToken> DiscoverableTokens { get; set; }
        public DbSet<Business> Businesses { get; set; }
        public DbSet<BusinessReceiveAddress> BusinessReceiveAddresses { get; set; }
        public DbSet<CryptoPayment> CryptoPayments { get; set; }

        //AppInstallation
        public DbSet<AppInstallation> AppInstallations { get; set; }

        //Agents
        public DbSet<AgentPlatformConnection> AgentPlatformConnections { get; set; }
        public DbSet<AgentAccount> AgentAccounts { get; set; }
        public DbSet<AgentSigningAuthority> AgentSigningAuthorities { get; set; }
        public DbSet<AgentAccountChallenge> AgentAccountChallenges { get; set; }
        public DbSet<AgentSpendingPolicy> AgentSpendingPolicies { get; set; }
        public DbSet<AgentSpendingPolicyAsset> AgentSpendingPolicyAssets { get; set; }
        // Security
        public DbSet<RequestNonce> RequestNonces { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Apply configurations
            builder.ApplyConfiguration(new BusinessConfiguration());
            builder.ApplyConfiguration(new CryptoPaymentConfiguration());
            //AppInstallation
            builder.ApplyConfiguration(new AppInstallationConfiguration());
            //Agents
            builder.ApplyConfiguration(new AgentAccountConfiguration());
            builder.ApplyConfiguration(new AgentPlatformConnectionConfiguration());
            builder.ApplyConfiguration(new AgentSigningAuthorityConfiguration());
            builder.ApplyConfiguration(new AgentAccountChallengeConfiguration());
            builder.ApplyConfiguration(new AgentSpendingPolicyConfiguration());
            builder.ApplyConfiguration(new AgentSpendingPolicyAssetConfiguration());
            //Security
            builder.ApplyConfiguration(new RequestNonceConfiguration());
        }
    }
}
