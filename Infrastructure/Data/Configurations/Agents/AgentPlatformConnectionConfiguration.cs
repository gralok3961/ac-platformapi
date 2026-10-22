using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations.Agents
{
    public class AgentPlatformConnectionConfiguration : IEntityTypeConfiguration<AgentPlatformConnection>
    {
        public void Configure(EntityTypeBuilder<AgentPlatformConnection> builder)
        {
            builder.ToTable("AgentPlatformConnections", "dbo", table =>
                table.HasCheckConstraint("CK_AgentPlatformConnections_Status", "[Status] IN (1, 2, 3)"));

            builder.HasKey(x => x.Id)
                   .HasName("PK_AgentPlatformConnections");

            builder.Property(x => x.Id)
                   .HasColumnType("uniqueidentifier")
                   .HasDefaultValueSql("NEWID()");

            builder.Property(x => x.AgentAccountId)
                   .HasColumnType("uniqueidentifier")
                   .IsRequired();

            builder.Property(x => x.PlatformProfileUrl)
                   .HasColumnType("nvarchar(512)")
                   .HasMaxLength(512)
                   .IsUnicode()
                   .UseCollation("Latin1_General_100_BIN2")
                   .IsRequired();

            builder.Property(x => x.Status)
                   .HasConversion<byte>()
                   .HasColumnType("tinyint")
                   .HasDefaultValue(AgentPlatformConnectionStatus.Active)
                   .IsRequired();

            builder.Property(x => x.ExpiresAt)
                   .HasColumnType("datetimeoffset(7)")
                   .IsRequired();

            builder.HasAlternateKey(x => new { x.AgentAccountId, x.PlatformProfileUrl })
                   .HasName("UQ_AgentPlatformConnections");

            builder.HasOne(x => x.AgentAccount)
                   .WithMany()
                   .HasForeignKey(x => x.AgentAccountId)
                   .HasConstraintName("FK_AgentPlatformConnections_AgentAccounts")
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
