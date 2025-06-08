namespace StoreManagement.Infrastructure.Data.Configurations;

public class SmsMessageEntityTypeConfiguration : IEntityTypeConfiguration<SmsMessage>
{
    public void Configure(EntityTypeBuilder<SmsMessage> builder)
    {
        
        builder.ToTable("SmsMessages");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .ValueGeneratedOnAdd()
            .IsRequired();
        builder.Ignore(c => c.DomainEvents);

        builder.OwnsOne(e => e.PhoneNumber, phoneBuilder =>
        {
            phoneBuilder.Property(p => p.Value).HasColumnName("PhoneNumber").HasMaxLength(11);
            phoneBuilder.WithOwner();
        }); 
        
        builder.Property(s => s.ProviderName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(s => s.Content).IsRequired();

        builder.Property(s => s.ErrorMessage).HasMaxLength(500);
        builder.Property(s => s.RetryCount).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.ParametersJson)
            .HasColumnName("Parameters")
            .HasColumnType("NVARCHAR(MAX)");

        builder.Property(e => e.TemplateId).IsRequired(false);
        builder.HasOne(e => e.Template)
            .WithMany()
            .HasForeignKey(e => e.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(sm => sm.SmsLogs)
            .WithOne(sl => sl.SmsMessage)
            .HasForeignKey(sl => sl.SmsMessageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}