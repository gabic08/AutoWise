using AutoWise.UserVehicles.Domain.Models;

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
