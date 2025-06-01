namespace StoreManagement.Infrastructure.Data.Configurations;

public class SmsLogEntityTypeConfiguration : IEntityTypeConfiguration<SmsLog>
{
    public void Configure(EntityTypeBuilder<SmsLog> builder)
    {
        builder.ToTable("SmsLogs");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Ignore(c => c.DomainEvents);

        builder.Property(s => s.SmsMessageId)
            .IsRequired();

        builder.Property(s => s.ProviderName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.LogType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(s => s.Data).IsRequired(false);

        builder.HasOne(sl=>sl.SmsMessage)
            .WithMany(sm => sm.SmsLogs)
            .HasForeignKey(sl => sl.SmsMessageId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}