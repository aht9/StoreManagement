namespace StoreManagement.Infrastructure.Data.Configurations;

public class SalesInvoiceEntityTypeConfiguration : IEntityTypeConfiguration<SalesInvoice>
{
    public void Configure(EntityTypeBuilder<SalesInvoice> builder)
    {
        builder.ToTable("SaleInvoices");
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

        builder.Property(pi => pi.CustomerId).IsRequired();

        builder.HasOne(pi => pi.Customer)
            .WithMany(s => s.SalesInvoices)
            .HasForeignKey(pi => pi.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        var navigation = builder.Metadata.FindNavigation(nameof(SalesInvoice.Items));
        navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}