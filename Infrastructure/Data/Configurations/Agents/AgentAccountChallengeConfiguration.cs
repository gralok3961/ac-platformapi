
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations.Agents
{
    public class AgentAccountChallengeConfiguration
        : IEntityTypeConfiguration<AgentAccountChallenge>
    {
        public void Configure(EntityTypeBuilder<AgentAccountChallenge> builder)
        {
            builder.ToTable("AgentAccountChallenges");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.Property(x => x.AppInstallationId)
                .IsRequired();

            builder.Property(x => x.ChallengeId)
                .HasMaxLength(64)
                .IsRequired();

            builder.Property(x => x.Purpose)
                .HasConversion<byte>()
                .HasColumnType("tinyint")
                .IsRequired();

            builder.Property(x => x.ChainId)
                .IsRequired();

            builder.Property(x => x.PublicAddress)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.Challenge)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            builder.Property(x => x.Nonce)
                .HasMaxLength(64)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasConversion<byte>()
                .HasColumnType("tinyint")
                .HasDefaultValue(AgentAccountChallengeStatus.Pending)
                .IsRequired();

            builder.Property(x => x.PublicKey)
                .HasMaxLength(300);

            builder.Property(x => x.Signature)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.ExpiresAt)
                .HasColumnType("datetimeoffset(7)")
                .IsRequired();

            builder.Property(x => x.VerifiedAt)
                .HasColumnType("datetimeoffset(7)");

            builder.Property(x => x.UsedAt)
                .HasColumnType("datetimeoffset(7)");

            builder.Property(x => x.DateCreated)
                .HasColumnType("datetimeoffset(7)")
                .HasDefaultValueSql("SYSDATETIMEOFFSET()")
                .IsRequired();

            builder.Property(x => x.DateUpdated)
                .HasColumnType("datetimeoffset(7)");

            builder.HasIndex(x => x.ChallengeId)
                .IsUnique();

            builder.HasOne<AppInstallation>()
                .WithMany()
                .HasForeignKey(x => x.AppInstallationId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}