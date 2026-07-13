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

    public static UserVehicleEvent Create(Guid userVehicleId, string name, string description, DateTime eventDate)
    {
        var vehicleEvent = new UserVehicleEvent
        {
            UserVehicleId = userVehicleId
        };
        vehicleEvent.SetName(name);
        vehicleEvent.Description = description?.Trim();
        vehicleEvent.EventDate = eventDate;

        return vehicleEvent;
    }

    public void Update(string name, string description, DateTime eventDate)
    {
        SetName(name);
        Description = description?.Trim();
        EventDate = eventDate;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }
        Name = name.Trim();
    }
}
