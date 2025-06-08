namespace StoreManagement.Infrastructure.Data.Configurations;

public class StoreEntityTypeConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.ToTable("Stores");
        builder.Ignore(s => s.DomainEvents);
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .IsRequired();
        builder.Property(e => e.Name).HasMaxLength(300);
        builder.Property(e => e.Location).HasMaxLength(500);
        builder.Property(e => e.ManagerName).HasMaxLength(200);
        builder.Property(e => e.ContactNumber).HasMaxLength(11);
        builder.Property(e => e.Email).HasMaxLength(200).IsRequired(false);
        builder.OwnsOne(e => e.Phone, phoneBuilder =>
        {
            phoneBuilder.Property(p => p.Value)
                .HasColumnName("PhoneNumber")
                .HasMaxLength(11);

            phoneBuilder.HasIndex(p => p.Value)
                .IsUnique()
                .HasDatabaseName("IX_Customers_PhoneNumber");

            phoneBuilder.WithOwner();
        });

        builder.OwnsOne(e => e.Address, addressBuilder =>
        {
            addressBuilder.Property(a => a.City).HasMaxLength(100).IsRequired();
            addressBuilder.Property(a => a.FullAddress).HasMaxLength(500).IsRequired();
            addressBuilder.WithOwner();
        });

        builder.HasIndex(c=>c.Name).IsUnique().HasDatabaseName("IX_Stores_Name");

        builder.HasMany(s => s.PurchaseInvoices)
            .WithOne(pi => pi.Store)
            .HasForeignKey(pi => pi.StoreId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

