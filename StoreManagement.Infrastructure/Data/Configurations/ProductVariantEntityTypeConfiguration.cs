namespace StoreManagement.Infrastructure.Data.Configurations;

public class ProductVariantEntityTypeConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("ProductVariants");
        builder.HasKey(pv => pv.Id);
        builder.Property(pv => pv.Id).ValueGeneratedOnAdd();
        builder.Property(pv => pv.SKU).IsRequired().HasMaxLength(50);
        builder.Property(pv => pv.Color).IsRequired().HasMaxLength(30);
        builder.Property(pv => pv.Size).IsRequired().HasMaxLength(20);
        builder.HasOne(pv => pv.Product)
            .WithMany(p => p.Variants)
            .HasForeignKey(pv => pv.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(pv => pv.SKU).IsUnique().HasDatabaseName("IX_ProductVariant_SKU");
    }
}