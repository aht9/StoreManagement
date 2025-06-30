namespace StoreManagement.Infrastructure.Data.Configurations;

public class InventoryTransactionTypeEntityTypeConfiguration : IEntityTypeConfiguration<InventoryTransactionType>
{
    public void Configure(EntityTypeBuilder<InventoryTransactionType> builder)
    {
        builder.ToTable("InventoryTransactionTypes");

        builder.HasKey(it => it.Id);

        builder.Property(o => o.Id)
            .ValueGeneratedNever();

        builder.Property(it=>it.Id)
            .HasDefaultValueSql("1")
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(it => it.Name)
            .IsRequired()
            .HasMaxLength(200);


        // Seed initial data
        builder.HasData(InventoryTransactionType.GetAll<InventoryTransactionType>());

    }
}