using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class CryptoPaymentConfiguration : IEntityTypeConfiguration<CryptoPayment>
    {
        public void Configure(EntityTypeBuilder<CryptoPayment> builder)
        {
            builder.ToTable("CryptoPayments");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.PaymentCode)
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(x => x.Status)
                   .IsRequired();

            builder.HasOne(x => x.Business)
                   .WithMany(b => b.CryptoPayments)
                   .HasForeignKey(x => x.BusinessId);

            builder.Property(x => x.CheckoutAmount)
                   .HasColumnType("decimal(18,2)");

            builder.Property(x => x.PaymentAmount)
                   .HasColumnType("decimal(28,8)");

            builder.Property(x => x.FxRate)
                   .HasColumnType("decimal(28,10)");

            builder.Property(x => x.SettlementAmount)
                   .HasColumnType("decimal(18,2)");

            builder.Property(x => x.FailureReason)
                   .HasColumnType("nvarchar(max)");

            builder.Property(x => x.CheckoutDeviceId)
                   .HasMaxLength(100);
        }
    }
}
