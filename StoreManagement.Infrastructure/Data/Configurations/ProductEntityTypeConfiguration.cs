namespace StoreManagement.Infrastructure.Data.Configurations;
public class ProductEntityTypeConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.Ignore(p => p.DomainEvents);
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .IsRequired();
        builder.Property(e => e.Name).HasMaxLength(300).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1000).IsRequired(false);

        builder.HasOne(p=>p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p=>p.Name).IsUnique().HasDatabaseName("IX_Products_Name");
    }
}

