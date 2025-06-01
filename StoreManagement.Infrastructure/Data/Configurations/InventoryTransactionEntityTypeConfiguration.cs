namespace StoreManagement.Infrastructure.Data.Configurations;

public class InventoryTransactionEntityTypeConfiguration : IEntityTypeConfiguration<InventoryTransaction>
{
    public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
    {
        builder.ToTable("InventoryTransactions");
        builder.Ignore(c => c.DomainEvents);


        builder.HasKey(it => it.Id);
        builder.Property(it => it.Id)
            .ValueGeneratedOnAdd()
            .IsRequired();
        builder.Property(it => it.ProductVariantId)
            .IsRequired();
        builder.Property(it => it.TransactionDate)
            .IsRequired()
            .HasColumnType("datetime2");
        builder.Property(it => it.Quantity)
            .IsRequired();

        builder
            .Property<int>("_transactionTypeId")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("TransactionTypeId")
            .IsRequired();

        builder.Property(it => it.ReferenceInvoiceId)
            .IsRequired(false);

        builder.Property(it => it.ReferenceInvoiceType)
            .IsRequired(false);

        builder.HasOne(it => it.ProductVariant)
            .WithMany()
            .HasForeignKey(it => it.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i=>i.TransactionType)
            .WithMany()
            .HasForeignKey("_transactionTypeId")
            .OnDelete(DeleteBehavior.Restrict);
    }
}