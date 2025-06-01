namespace StoreManagement.Infrastructure.Data.Configurations;

public class PurchaseInvoiceEntityTypeConfiguration : IEntityTypeConfiguration<PurchaseInvoice>
{
    public void Configure(EntityTypeBuilder<PurchaseInvoice> builder)
    {
        builder.ToTable("PurchaseInvoices");
        builder.Ignore(c => c.DomainEvents);

        builder.HasKey(pi => pi.Id);
        builder.Property(pi => pi.Id)
            .ValueGeneratedOnAdd();

        builder.Property(pi => pi.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(pi => pi.InvoiceDate)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(pi => pi.TotalAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(pi => pi.PaidAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(pi => pi.DueAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired()
            .HasComputedColumnSql("[TotalAmount] - [PaidAmount]");

        builder.Property(pi => pi.PaymentType).IsRequired();

        builder.Property(pi => pi.InvoiceStatus).IsRequired();

        builder.Property(pi => pi.StoreId).IsRequired();

        builder.HasOne(pi => pi.Store)
            .WithMany(s => s.PurchaseInvoices)
            .HasForeignKey(pi => pi.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        var navigation = builder.Metadata.FindNavigation(nameof(PurchaseInvoice.Items));
        navigation.SetPropertyAccessMode(PropertyAccessMode.Field);

    }
}