using Core.Entities.Security;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RequestNonceConfiguration : IEntityTypeConfiguration<RequestNonce>
{
    public void Configure(EntityTypeBuilder<RequestNonce> builder)
    {
        builder.ToTable("RequestNonces", "dbo", table =>
        {
            table.HasCheckConstraint(
                "CK_RequestNonces_ExpiresAt",
                "[ExpiresAt] > [DateCreated]"
            );
        });

        builder.HasKey(x => x.Id)
            .HasName("PK_RequestNonces");

        builder.Property(x => x.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Scope)
            .IsRequired()
            .HasMaxLength(100)
            .UseCollation("Latin1_General_100_BIN2");

        builder.Property(x => x.Nonce)
            .IsRequired()
            .HasMaxLength(128)
            .UseCollation("Latin1_General_100_BIN2");

        builder.Property(x => x.DateCreated)
            .HasColumnType("datetimeoffset(7)")
            .HasDefaultValueSql(
                "TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')"
            )
            .ValueGeneratedOnAdd();

        builder.Property(x => x.ExpiresAt)
            .IsRequired()
            .HasColumnType("datetimeoffset(7)");

        builder.HasIndex(x => new { x.Scope, x.Nonce })
            .IsUnique()
            .HasDatabaseName("UX_RequestNonces_Scope_Nonce");
    }
}