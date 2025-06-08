namespace StoreManagement.Infrastructure.Data.Configurations;

public class SmsProviderEntityTypeConfiguration : IEntityTypeConfiguration<SmsProvider>
{
    public void Configure(EntityTypeBuilder<SmsProvider> builder)
    {
        builder.ToTable("SmsProviders");
        builder.Ignore(c => c.DomainEvents);
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s=>s.TypeName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s=>s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.Priority)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.SettingsJson)
            .HasColumnName("Settings")
            .HasColumnType("NVARCHAR(MAX)");
    }
}