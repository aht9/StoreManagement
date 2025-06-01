namespace StoreManagement.Infrastructure.Data.Configurations;

public class InstallmentEntityTypeConfiguration : IEntityTypeConfiguration<Installment>
{
    public void Configure(EntityTypeBuilder<Installment> builder)
    {
        builder.ToTable("Installments");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id)
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property(i => i.InvoiceId)
            .IsRequired();
        builder.Property(i => i.InvoiceType)
            .IsRequired();
        builder.Property(i => i.InstallmentNumber)
            .IsRequired();
        builder.Property(i => i.DueDate)
            .IsRequired()
            .HasColumnType("datetime2");
        builder.Property(i => i.AmountDue)
            .IsRequired()
            .HasColumnType("decimal(18,2)");
        builder.Property(i => i.AmountPaid)
            .IsRequired()
            .HasColumnType("decimal(18,2)");
        builder.Property(i => i.Status)
            .IsRequired();

    }
}