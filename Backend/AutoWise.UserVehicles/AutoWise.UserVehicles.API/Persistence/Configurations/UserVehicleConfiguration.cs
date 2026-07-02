using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWise.UserVehicles.API.Persistence.Configurations;

public class UserVehicleConfiguration : ModifiedCreatedAuditEntityConfiguration<UserVehicle>
{
    public override void Configure(EntityTypeBuilder<UserVehicle> builder)
    {
        base.Configure(builder);
        builder.ToTable(nameof(UserVehicle), "UserVehicles");

        builder.Property(uv => uv.LicensePlateNumber).IsRequired();
    }
}
