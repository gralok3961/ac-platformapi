using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Infrastructure.Data.Configurations.Agents
{
    public class AgentAccountConfiguration : IEntityTypeConfiguration<AgentAccount>
    {
        public void Configure(EntityTypeBuilder<AgentAccount> builder)
        {
            builder.ToTable("AgentAccounts", "dbo");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                   .HasDefaultValueSql("NEWID()");

            builder.Property(x => x.PublicId)
                   .HasMaxLength(30)
                   .IsUnicode(false)
                   .UseCollation("Latin1_General_100_BIN2")
                   .IsRequired()
                   .ValueGeneratedNever()
                   .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);

            builder.HasIndex(x => x.PublicId)
                   .IsUnique()
                   .HasDatabaseName("UX_AgentAccounts_PublicId");

            builder.Property(x => x.ChainId)
                   .IsRequired();

            builder.Property(x => x.PublicAddress)
                   .HasMaxLength(150)
                   .UseCollation("Latin1_General_100_BIN2")
                   .IsRequired();

            builder.Property(x => x.Status)
                   .HasConversion<byte>()
                   .HasDefaultValue(AgentAccountStatus.Active)
                   .IsRequired();

            builder.Property(x => x.DateCreated)
                   .HasColumnType("datetimeoffset(7)")
                   .HasDefaultValueSql("SYSDATETIMEOFFSET()")
                   .IsRequired();

            builder.Property(x => x.DateUpdated)
                   .HasColumnType("datetimeoffset(7)");

            builder.Property(x => x.DeletedAt)
                   .HasColumnType("datetimeoffset(7)");

            builder.HasOne(x => x.AppInstallation)
                   .WithMany(x => x.AgentAccounts)
                   .HasForeignKey(x => x.AppInstallationId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.ChainId, x.PublicAddress })
                   .IsUnique()
                   .HasDatabaseName("UX_AgentAccounts_Network_PublicAddress")
                   .HasFilter("[DeletedAt] IS NULL");

            builder.HasIndex(x => x.AppInstallationId)
                   .HasDatabaseName("IX_AgentAccounts_AppInstallationId")
                   .HasFilter("[DeletedAt] IS NULL");
        }
    }
}
