using AutoWise.CommonUtilities.Persistence.PostgreSQL.Configurations;
using AutoWise.UserVehicles.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWise.UserVehicles.Infrastructure.Data.Configurations;

public class UserVehicleEventConfiguration : ModifiedCreatedAuditEntityConfiguration<UserVehicleEvent>
{
    public override void Configure(EntityTypeBuilder<UserVehicleEvent> builder)
    {
        base.Configure(builder);
        builder.ToTable(nameof(UserVehicleEvent), InfrastructureDataConstants.UserVehiclesSchema);

        builder.Property(uve => uve.Name).IsRequired();
        builder.Property(uve => uve.EventDate).IsRequired();
        builder.Property(uve => uve.UserVehicleId).IsRequired();
    }
}