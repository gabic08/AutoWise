using AutoWise.CommonUtilities.Models.BaseEntities;

namespace AutoWise.UserVehicles.Domain.Models;

public class UserVehicle : ModifiedCreatedAuditBaseEntity
{
    public string LicensePlateNumber { get; private set; }
    public string Make { get; private set; }
    public string Model { get; private set; }
    public Guid UserId { get; private set; }
    public string Vin { get; private set; }
    public int? Year { get; private set; }

    private readonly List<UserVehicleEvent> _userVehicleEvents = [];
    public IReadOnlyCollection<UserVehicleEvent> UserVehicleEvents => _userVehicleEvents.AsReadOnly();

    private UserVehicle() { }

    public static UserVehicle Create(Guid userId, string licensePlateNumber, string make, string model, string vin, int? year)
    {
        var vehicle = new UserVehicle
        {
            UserId = userId
        };
        vehicle.ChangeLicensePlateNumber(licensePlateNumber);
        vehicle.SetMake(make);
        vehicle.SetModel(model);
        vehicle.SetVin(vin);
        vehicle.SetYear(year);

        return vehicle;
    }

    public void ChangeLicensePlateNumber(string licensePlateNumber)
    {
        if (string.IsNullOrWhiteSpace(licensePlateNumber))
        {
            throw new ArgumentException("License plate is required.", nameof(licensePlateNumber));
        }

        LicensePlateNumber = licensePlateNumber.Trim();
    }

    private void SetMake(string make)
    {
        if (string.IsNullOrWhiteSpace(make))
        {
            throw new ArgumentException("Make is required.", nameof(make));
        }
        Make = make.Trim();
    }

    private void SetModel(string model)
    {
        if (string.IsNullOrWhiteSpace(model))
        {
            throw new ArgumentException("Model is required.", nameof(model));
        }
        Model = model.Trim();
    }

    private void SetVin(string vin)
    {
        if (string.IsNullOrWhiteSpace(vin))
        {
            throw new ArgumentException("VIN is required.", nameof(vin));
        }
        if (vin.Length != 17)
        {
            throw new ArgumentOutOfRangeException(nameof(vin), "VIN must be 17 characters long.");
        }
        Vin = vin.Trim();
    }

    private void SetYear(int? year)
    {
        if (year.HasValue && year > DateTime.UtcNow.Year + 1)
        {
            throw new ArgumentOutOfRangeException(nameof(year), "Year must be lower than next year.");
        }
        Year = year;
    }

    public UserVehicleEvent AddEvent(string name, string description, DateTime eventDate)
    {
        var vehicleEvent = UserVehicleEvent.Create(Id, name, description, eventDate);
        _userVehicleEvents.Add(vehicleEvent);

        return vehicleEvent;
    }

    public void UpdateEvent(Guid eventId, string name, string description, DateTime eventDate)
    {
        var vehicleEvent = _userVehicleEvents.FirstOrDefault(e => e.Id == eventId)
            ?? throw new InvalidOperationException($"Vehicle event with id '{eventId}' does not belong to this vehicle.");

        vehicleEvent.Update(name, description, eventDate);
    }

    public void RemoveEvent(Guid eventId)
    {
        var vehicleEvent = _userVehicleEvents.FirstOrDefault(e => e.Id == eventId)
            ?? throw new InvalidOperationException($"Vehicle event with id '{eventId}' does not belong to this vehicle.");

        _userVehicleEvents.Remove(vehicleEvent);
    }
}
