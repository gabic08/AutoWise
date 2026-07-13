using AutoWise.UserVehicles.Domain.Models;

namespace AutoWise.UserVehicles.Tests.Domain;

public class UserVehicleTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private const string ValidVin = "1HGCM82633A004352";

    [Fact]
    public void Create_WithValidData_SetsAllProperties()
    {
        // Act
        var vehicle = UserVehicle.Create(UserId, "ABC-123", "Toyota", "Corolla", ValidVin, 2020);

        // Assert
        Assert.Equal(UserId, vehicle.UserId);
        Assert.Equal("ABC-123", vehicle.LicensePlateNumber);
        Assert.Equal("Toyota", vehicle.Make);
        Assert.Equal("Corolla", vehicle.Model);
        Assert.Equal(ValidVin, vehicle.Vin);
        Assert.Equal(2020, vehicle.Year);
        Assert.Empty(vehicle.UserVehicleEvents);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidLicensePlate_ThrowsArgumentException(string? licensePlate)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            UserVehicle.Create(UserId, licensePlate!, "Toyota", "Corolla", ValidVin, 2020));
    }

    [Fact]
    public void Create_WithInvalidVinLength_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            UserVehicle.Create(UserId, "ABC-123", "Toyota", "Corolla", "SHORTVIN", 2020));
    }

    [Fact]
    public void Create_WithYearTooFarInFuture_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidYear = DateTime.UtcNow.Year + 2;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            UserVehicle.Create(UserId, "ABC-123", "Toyota", "Corolla", ValidVin, invalidYear));
    }

    [Fact]
    public void ChangeLicensePlateNumber_WithValidValue_TrimsAndUpdates()
    {
        // Arrange
        var vehicle = UserVehicle.Create(UserId, "ABC-123", "Toyota", "Corolla", ValidVin, 2020);

        // Act
        vehicle.ChangeLicensePlateNumber("  XYZ-999  ");

        // Assert
        Assert.Equal("XYZ-999", vehicle.LicensePlateNumber);
    }

    [Fact]
    public void ChangeLicensePlateNumber_WithEmptyValue_ThrowsArgumentException()
    {
        // Arrange
        var vehicle = UserVehicle.Create(UserId, "ABC-123", "Toyota", "Corolla", ValidVin, 2020);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => vehicle.ChangeLicensePlateNumber(" "));
    }

    [Fact]
    public void AddEvent_AddsEventToCollection()
    {
        // Arrange
        var vehicle = UserVehicle.Create(UserId, "ABC-123", "Toyota", "Corolla", ValidVin, 2020);

        // Act
        var addedEvent = vehicle.AddEvent("Oil Change", "Full synthetic oil change", DateTime.UtcNow);

        // Assert
        Assert.Single(vehicle.UserVehicleEvents);
        Assert.Same(addedEvent, vehicle.UserVehicleEvents.Single());
        Assert.Equal(vehicle.Id, addedEvent.UserVehicleId);
    }

    [Fact]
    public void UpdateEvent_WithExistingEventId_UpdatesFields()
    {
        // Arrange
        var vehicle = UserVehicle.Create(UserId, "ABC-123", "Toyota", "Corolla", ValidVin, 2020);
        var addedEvent = vehicle.AddEvent("Oil Change", "Full synthetic", DateTime.UtcNow);
        var newDate = DateTime.UtcNow.AddDays(1);

        // Act
        vehicle.UpdateEvent(addedEvent.Id, "Tire Rotation", "Rotated all four tires", newDate);

        // Assert
        Assert.Equal("Tire Rotation", addedEvent.Name);
        Assert.Equal("Rotated all four tires", addedEvent.Description);
        Assert.Equal(newDate, addedEvent.EventDate);
    }

    [Fact]
    public void UpdateEvent_WithUnknownEventId_ThrowsInvalidOperationException()
    {
        // Arrange
        var vehicle = UserVehicle.Create(UserId, "ABC-123", "Toyota", "Corolla", ValidVin, 2020);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            vehicle.UpdateEvent(Guid.NewGuid(), "Tire Rotation", "Rotated all four tires", DateTime.UtcNow));
    }

    [Fact]
    public void RemoveEvent_WithExistingEventId_RemovesFromCollection()
    {
        // Arrange
        var vehicle = UserVehicle.Create(UserId, "ABC-123", "Toyota", "Corolla", ValidVin, 2020);
        var addedEvent = vehicle.AddEvent("Oil Change", "Full synthetic", DateTime.UtcNow);

        // Act
        vehicle.RemoveEvent(addedEvent.Id);

        // Assert
        Assert.Empty(vehicle.UserVehicleEvents);
    }

    [Fact]
    public void RemoveEvent_WithUnknownEventId_ThrowsInvalidOperationException()
    {
        // Arrange
        var vehicle = UserVehicle.Create(UserId, "ABC-123", "Toyota", "Corolla", ValidVin, 2020);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => vehicle.RemoveEvent(Guid.NewGuid()));
    }
}
