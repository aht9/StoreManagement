namespace StoreManagement.Infrastructure.Data.Configurations;

public class SmsTemplateEntityTypeConfiguration : IEntityTypeConfiguration<SmsTemplate>
{
    public void Configure(EntityTypeBuilder<SmsTemplate> builder)
    {
        builder.ToTable("SmsTemplates");
        builder.Ignore(c => c.DomainEvents);

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s=>s.Content)
            .IsRequired();

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.DefaultParametersJson)
            .HasColumnName("DefaultParameters")
            .HasColumnType("jsonb");

        
    }
}