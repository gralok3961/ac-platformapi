using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations.Agents
{
    public class AgentSpendingPolicyAssetConfiguration : IEntityTypeConfiguration<AgentSpendingPolicyAsset>
    {
        public void Configure(EntityTypeBuilder<AgentSpendingPolicyAsset> builder)
        {
            builder.ToTable("AgentSpendingPolicyAssets", "dbo");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                   .HasDefaultValueSql("NEWID()");

            builder.Property(x => x.CurrencyCode)
                   .HasMaxLength(80)
                   .UseCollation("Latin1_General_100_BIN2")
                   .IsRequired();

            builder.Property(x => x.Issuer)
                   .HasMaxLength(200)
                   .UseCollation("Latin1_General_100_BIN2");

            builder.Property(x => x.PerTransactionLimit)
                   .HasColumnType("decimal(38,18)")
                   .IsRequired();

            builder.Property(x => x.DateCreated)
                   .HasColumnType("datetimeoffset(7)")
                   .HasDefaultValueSql("SYSDATETIMEOFFSET()")
                   .IsRequired();

            builder.Property(x => x.DateUpdated)
                   .HasColumnType("datetimeoffset(7)");

            builder.Property(x => x.DeletedAt)
                   .HasColumnType("datetimeoffset(7)");

            builder.HasOne(x => x.AgentSpendingPolicy)
                   .WithMany(x => x.Assets)
                   .HasForeignKey(x => x.AgentSpendingPolicyId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.AgentSpendingPolicyId, x.CurrencyCode, x.Issuer })
                   .IsUnique()
                   .HasDatabaseName("UX_AgentSpendingPolicyAssets_Policy_Asset")
                   .HasFilter("[DeletedAt] IS NULL");

            builder.HasIndex(x => x.AgentSpendingPolicyId)
                   .HasDatabaseName("IX_AgentSpendingPolicyAssets_AgentSpendingPolicyId")
                   .HasFilter("[DeletedAt] IS NULL");
        }
    }
}
