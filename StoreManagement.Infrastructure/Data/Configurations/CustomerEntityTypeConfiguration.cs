namespace StoreManagement.Infrastructure.Data.Configurations;

    public class CustomerEntityTypeConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.Ignore(c => c.DomainEvents);

        builder.HasKey(c => c.Id);
        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .IsRequired();
        builder.Property(c => c.FirstName).HasMaxLength(250).IsRequired();
        builder.Property(c => c.LastName).HasMaxLength(250).IsRequired();
        builder.Property(c => c.Email)
            .HasMaxLength(250)
            .IsRequired(false)
            .HasAnnotation("DataType", DataType.EmailAddress);


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


        builder.OwnsOne(c => c.Address, address =>
        {
            address.Property(a => a.City).HasMaxLength(100).IsRequired();
            address.Property(a => a.FullAddress).HasMaxLength(500).IsRequired();
            address.WithOwner();
        });
        builder.Property(c => c.NationalCode).HasMaxLength(11).IsRequired(false);
        builder.Property(c => c.DateOfBirth)
            .HasColumnType("datetime2")
            .IsRequired();


        builder.HasMany(c => c.SalesInvoices)
            .WithOne(pi => pi.Customer)
            .HasForeignKey(pi => pi.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}