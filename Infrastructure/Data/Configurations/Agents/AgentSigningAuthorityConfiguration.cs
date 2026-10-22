using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations.Agents
{
    public class AgentSigningAuthorityConfiguration : IEntityTypeConfiguration<AgentSigningAuthority>
    {
        public void Configure(EntityTypeBuilder<AgentSigningAuthority> builder)
        {
            builder.ToTable("AgentSigningAuthorities", "dbo");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                   .HasDefaultValueSql("NEWID()");

            builder.Property(x => x.Provider)
                   .HasConversion<byte>()
                   .IsRequired();

            builder.Property(x => x.ProviderKeyReference)
                   .HasColumnType("VARCHAR(500)")
                   .HasMaxLength(500)
                   .IsUnicode(false)
                   .IsRequired();

            builder.Property(x => x.PublicKey)
                   .HasMaxLength(300)
                   .IsRequired();

            builder.Property(x => x.DelegatedAuthorityIdentifier)
                   .HasMaxLength(200)
                   .IsRequired();

            builder.Property(x => x.Status)
                   .HasConversion<byte>()
                   .HasDefaultValue(AgentSigningAuthorityStatus.Provisioned)
                   .IsRequired();

            builder.Property(x => x.ActivatedAt)
                   .HasColumnType("datetimeoffset(7)");

            builder.Property(x => x.RevokedAt)
                   .HasColumnType("datetimeoffset(7)");

            builder.Property(x => x.DateCreated)
                   .HasColumnType("datetimeoffset(7)")
                   .HasDefaultValueSql("SYSDATETIMEOFFSET()")
                   .IsRequired();

            builder.Property(x => x.DateUpdated)
                   .HasColumnType("datetimeoffset(7)");

            builder.Property(x => x.DeletedAt)
                   .HasColumnType("datetimeoffset(7)");

            builder.HasOne(x => x.AgentAccount)
                   .WithMany(x => x.SigningAuthorities)
                   .HasForeignKey(x => x.AgentAccountId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.AgentAccountId)
                   .HasDatabaseName("IX_AgentSigningAuthorities_AgentAccountId")
                   .HasFilter("[DeletedAt] IS NULL");

            builder.HasIndex(x => x.AgentAccountId, "UX_AgentSigningAuthorities_Current_AgentAccountId")
                   .IsUnique()
                   .HasDatabaseName("UX_AgentSigningAuthorities_Current_AgentAccountId")
                   .HasFilter("[Status] IN (1, 2) AND [DeletedAt] IS NULL");

            builder.HasIndex(x => x.ProviderKeyReference)
                   .IsUnique()
                   .HasDatabaseName("UX_AgentSigningAuthorities_ProviderKeyReference")
                   .HasFilter("[DeletedAt] IS NULL");

            builder.HasIndex(x => x.DelegatedAuthorityIdentifier)
                   .HasDatabaseName("IX_AgentSigningAuthorities_DelegatedAuthorityIdentifier")
                   .HasFilter("[DeletedAt] IS NULL");
        }
    }
}
