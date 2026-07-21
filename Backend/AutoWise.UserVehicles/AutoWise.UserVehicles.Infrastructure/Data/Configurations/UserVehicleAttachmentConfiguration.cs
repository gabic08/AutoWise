namespace AutoWise.UserVehicles.Infrastructure.Data.Configurations;

public class UserVehicleAttachmentConfiguration : CreatedAuditEntityConfiguration<UserVehicleAttachment>
{
    public override void Configure(EntityTypeBuilder<UserVehicleAttachment> builder)
    {
        base.Configure(builder);
        builder.ToTable(nameof(UserVehicleAttachment), InfrastructureDataConstants.UserVehiclesSchema);

        builder.Property(ua => ua.UserVehicleId).IsRequired();
        builder.Property(ua => ua.MediaAttachmentId).IsRequired();
        builder.Property(ua => ua.OriginalFileName).IsRequired();
        builder.Property(ua => ua.ContentType).IsRequired();
        builder.Property(ua => ua.SizeInBytes).IsRequired();

        builder.HasIndex(ua => ua.MediaAttachmentId).IsUnique();
    }
}