namespace StoreManagement.Infrastructure.Data.Configurations;

public class FinancialTransactionEntityTypeConfiguration : IEntityTypeConfiguration<FinancialTransaction>
{
    public void Configure(EntityTypeBuilder<FinancialTransaction> builder)
    {
        builder.ToTable("FinancialTransactions");
        builder.Ignore(c => c.DomainEvents);


        builder.HasKey(ft => ft.Id);
        builder.Property(ft => ft.Id)
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property(ft => ft.Amount).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(ft => ft.TransactionDate).IsRequired().HasColumnType("datetime2");
        builder.Property(ft => ft.Description).HasMaxLength(500);

        builder.Property(ft => ft.InvoiceId)
            .IsRequired(false)
            .HasColumnType("bigint");

        builder.Property(ft => ft.InvoiceType).IsRequired(false);

        builder.HasOne(ft => ft.BankAccount)
            .WithMany(b => b.Transactions)
            .HasForeignKey(ft => ft.BankAccountId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}