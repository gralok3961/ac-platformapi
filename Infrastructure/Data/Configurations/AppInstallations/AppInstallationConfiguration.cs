using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations.AppInstallations
{
    public class AppInstallationConfiguration : IEntityTypeConfiguration<AppInstallation>
    {
        public void Configure(EntityTypeBuilder<AppInstallation> builder)
        {
            builder.ToTable("AppInstallations");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Platform)
                   .HasConversion<byte>()
                   .IsRequired();

            builder.Property(x => x.Status)
                   .HasConversion<byte>()
                   .HasDefaultValue(AppInstallationStatus.Active)
                   .IsRequired();

            builder.Property(x => x.AppVersion)
                   .HasMaxLength(50);

            builder.Property(x => x.OsVersion)
                   .HasMaxLength(100);

            builder.Property(x => x.DeviceModel)
                   .HasMaxLength(150);

            builder.Property(x => x.FirstSeenIp)
                   .HasMaxLength(45);

            builder.Property(x => x.LastSeenIp)
                   .HasMaxLength(45);

            builder.Property(x => x.FirstSeenCountry)
                   .HasMaxLength(10);

            builder.Property(x => x.LastSeenCountry)
                   .HasMaxLength(10);

            builder.Property(x => x.DateCreated)
                   .HasDefaultValueSql("SYSDATETIMEOFFSET()")
                   .IsRequired();

            builder.HasIndex(x => x.Status)
                   .HasDatabaseName("IX_AppInstallations_Status")
                   .HasFilter("[DeletedAt] IS NULL");
        }
    }
}