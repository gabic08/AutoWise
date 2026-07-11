using AutoWise.CommonUtilities.Persistence.PostgreSQL.Configurations;
using AutoWise.UserVehicles.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWise.UserVehicles.Infrastructure.Data.Configurations;

public class UserVehicleConfiguration : ModifiedCreatedAuditEntityConfiguration<UserVehicle>
{
    public override void Configure(EntityTypeBuilder<UserVehicle> builder)
    {
        base.Configure(builder);
        builder.ToTable(nameof(UserVehicle), InfrastructureDataConstants.UserVehiclesSchema);

        builder.Property(uv => uv.UserId).IsRequired();
        builder.Property(uv => uv.LicensePlateNumber).IsRequired();

        builder.HasMany(uv => uv.UserVehicleEvents)
            .WithOne(uve => uve.UserVehicle)
            .HasForeignKey(uve => uve.UserVehicleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}