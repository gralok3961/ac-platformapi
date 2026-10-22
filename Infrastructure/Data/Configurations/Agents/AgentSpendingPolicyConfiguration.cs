using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations.Agents
{
    public class AgentSpendingPolicyConfiguration : IEntityTypeConfiguration<AgentSpendingPolicy>
    {
        public void Configure(EntityTypeBuilder<AgentSpendingPolicy> builder)
        {
            builder.ToTable("AgentSpendingPolicies", "dbo");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                   .HasDefaultValueSql("NEWID()");

            builder.Property(x => x.ExpiresAt)
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
                   .WithMany(x => x.SpendingPolicies)
                   .HasForeignKey(x => x.AgentAccountId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.AgentAccountId)
                   .IsUnique()
                   .HasDatabaseName("UX_AgentSpendingPolicies_AgentAccountId")
                   .HasFilter("[DeletedAt] IS NULL");
        }
    }
}
