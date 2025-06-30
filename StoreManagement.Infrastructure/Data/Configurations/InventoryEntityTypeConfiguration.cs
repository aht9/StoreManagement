namespace StoreManagement.Infrastructure.Data.Configurations;

public class InventoryEntityTypeConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> builder)
    {
        builder.ToTable("Inventories");
        builder.Ignore(e => e.DomainEvents);

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .ValueGeneratedOnAdd()
            .IsRequired();

        //builder.HasIndex(i => i.ProductVariantId)
        //    .IsUnique();

        builder.Property(i => i.ProductVariantId)
            .IsRequired();

        builder.Property(i => i.Quantity)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasOne<Domain.Aggregates.Products.ProductVariant>()
            .WithOne() 
            .HasForeignKey<Inventory>(i => i.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade); 
    }
}