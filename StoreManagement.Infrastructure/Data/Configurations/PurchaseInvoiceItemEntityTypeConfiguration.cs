namespace StoreManagement.Infrastructure.Data.Configurations;

public class PurchaseInvoiceItemEntityTypeConfiguration : IEntityTypeConfiguration<PurchaseInvoiceItem>
{
    public void Configure(EntityTypeBuilder<PurchaseInvoiceItem> builder)
    {
        builder.ToTable("PurchaseInvoiceItems");
        builder.Ignore(c => c.DomainEvents);


        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd()
            .HasColumnName("Id")
            .IsRequired();

        builder.Property(x => x.PurchaseInvoiceId)
            .HasColumnName("PurchaseInvoiceId")
            .IsRequired();
        builder.Property(x => x.ProductVariantId)
            .HasColumnName("ProductVariantId")
            .IsRequired();
        builder.Property(x => x.Quantity)
            .HasColumnName("Quantity")
            .IsRequired();
        builder.Property(x => x.UnitPrice)
            .HasColumnName("UnitPrice")
            .IsRequired()
            .HasColumnType("decimal(18,2)");
        builder.Property(x => x.Discount)
            .HasColumnName("Discount")
            .IsRequired();
        builder.Property(x => x.Tax)
            .HasColumnName("Tax")
            .IsRequired();
        builder.Property(x => x.TotalPrice)
            .HasColumnName("TotalPrice")
            .IsRequired()
            .HasColumnType("decimal(18,2)");
    }
}