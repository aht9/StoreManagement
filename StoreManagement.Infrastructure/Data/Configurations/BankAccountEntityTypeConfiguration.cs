namespace StoreManagement.Infrastructure.Data.Configurations;

public class BankAccountEntityTypeConfiguration : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> builder)
    {
        builder.ToTable("BankAccounts");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id)
            .ValueGeneratedOnAdd()
            .IsRequired();
        builder.Ignore(c => c.DomainEvents);

        builder.Property(b => b.AccountName).IsRequired().HasMaxLength(100);
        builder.Property(b => b.BankName).IsRequired().HasMaxLength(100);
        builder.Property(b => b.AccountNumber).IsRequired().HasMaxLength(50);
        builder.Property(b => b.CardNumberLastFour).IsRequired().HasMaxLength(4);
        builder.Property(b => b.Balance).HasColumnType("decimal(18,2)").HasDefaultValue(0);
        // Configure the one-to-many relationship with FinancialTransaction
        builder.HasMany(b => b.Transactions)
            .WithOne(t => t.BankAccount)
            .HasForeignKey(t => t.BankAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure the index for AccountNumber to ensure uniqueness
        builder.HasIndex(b => b.AccountNumber).IsUnique().HasDatabaseName("IX_BankAccounts_AccountNumber");
        builder.HasIndex(b => b.CardNumberLastFour).HasDatabaseName("IX_BankAccounts_CardNumberLastFour");
    }
}