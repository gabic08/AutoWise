using AutoWise.CommonUtilities.Models.BaseEntities;

namespace AutoWise.UserVehicles.Domain.Models;

public class UserVehicleEvent : ModifiedCreatedAuditBaseEntity
{
    public string Description { get; private set; }
    public DateTime EventDate { get; private set; }
    public string Name { get; private set; }
    public UserVehicle UserVehicle { get; private set; }
    public Guid UserVehicleId { get; private set; }

    private UserVehicleEvent() { }
}
