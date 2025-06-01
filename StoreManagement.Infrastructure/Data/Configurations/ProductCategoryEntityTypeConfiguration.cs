namespace StoreManagement.Infrastructure.Data.Configurations;

public class ProductCategoryEntityTypeConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable("ProductCategories");
        builder.Ignore(pc => pc.DomainEvents);
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .IsRequired();
        builder.Property(e => e.Name).HasMaxLength(300).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1000).IsRequired(false);
        
        builder.HasMany(pc=>pc.Products)
            .WithOne(p => p.Category)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(pc => pc.Name).IsUnique().HasDatabaseName("IX_ProductCategories_Name");
    }
}