﻿using StoreManagement.Infrastructure.Data.Configurations;

namespace StoreManagement.Infrastructure.Data.DbContexts;

public class ApplicationDbContext : DbContext, IUnitOfWork
{
    private readonly IMediator? _mediator;
    private IDbContextTransaction? _currentTransaction;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IMediator mediator) : base(options)
    {
        _mediator = mediator;
    }

    // This constructor is used by the DesignTimeFactory and does not require IMediator.
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        _mediator = null;
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Store> Stores { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<PurchaseInvoice> PurchaseInvoices { get; set; }
    public DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems { get; set; }
    public DbSet<SalesInvoice> SalesInvoices { get; set; }
    public DbSet<SalesInvoiceItem> SalesInvoiceItems { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
    public DbSet<BankAccount> BankAccounts { get; set; }
    public DbSet<FinancialTransaction> FinancialTransactions { get; set; }
    public DbSet<Installment> Installments { get; set; }
    public DbSet<InventoryTransactionType> InventoryTransactionTypes { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<SmsLog> SmsLogs { get; set; }
    public DbSet<SmsMessage> SmsMessages { get; set; }
    public DbSet<SmsTemplate> SmsTemplates { get; set; }
    public DbSet<SmsProvider> SmsProviders { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new CustomerEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new StoreEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new ProductEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new ProductVariantEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new PurchaseInvoiceEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new PurchaseInvoiceItemEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new SalesInvoiceEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new SalesInvoiceItemEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new InventoryEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new InventoryTransactionEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new InventoryTransactionTypeEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new BankAccountEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new FinancialTransactionEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new InstallmentEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new ProductCategoryEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new SmsLogEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new SmsMessageEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new SmsTemplateEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new SmsProviderEntityTypeConfiguration());

    }

    public IDbContextTransaction? GetCurrentTransaction() => _currentTransaction;

    public bool HasActiveTransaction => _currentTransaction != null;

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        if (_currentTransaction != null) return null;

        _currentTransaction = await Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);

        return _currentTransaction;
    }

    public async Task CommitTransactionAsync(IDbContextTransaction transaction)
    {
        if (transaction == null) throw new ArgumentNullException(nameof(transaction));

        if (transaction != _currentTransaction)
            throw new InvalidOperationException($"Transaction {transaction.TransactionId} is not current");

        try
        {
            await SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        try
        {
            await _currentTransaction?.RollbackAsync();
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default(CancellationToken))
    {
        try
        {
            if (_mediator != null)
            {
                await _mediator.DispatchDomainEventsAsync(this);
            }
            var result = await base.SaveChangesAsync(cancellationToken) > 0;
            return result;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return false;
        }
        catch (DbUpdateException ex)
        {
            return false;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}