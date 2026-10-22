using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class BusinessConfiguration : IEntityTypeConfiguration<Business>
    {
        public void Configure(EntityTypeBuilder<Business> builder)
        {
            builder.ToTable("Businesses");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.PublicId)
                .HasMaxLength(36)
                .IsUnicode(false)
                .IsRequired()
                .HasDefaultValueSql("'biz_' + LOWER(REPLACE(CONVERT(VARCHAR(36), NEWID()), '-', ''))")
                .ValueGeneratedOnAdd();

            builder.HasIndex(x => x.PublicId)
                .HasDatabaseName("UX_Businesses_PublicId")
                .IsUnique();

            builder.Property(x => x.BusinessName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.BusinessLogoUrl).HasMaxLength(500);

            builder.HasOne(x => x.User)
                   .WithOne(u => u.Business)
                   .HasForeignKey<Business>(x => x.UserId);
        }
    }
}
