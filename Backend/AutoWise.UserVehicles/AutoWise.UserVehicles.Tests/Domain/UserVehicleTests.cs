using AutoWise.UserVehicles.Domain.Models;
using FluentAssertions;

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
        vehicle.UserId.Should().Be(UserId);
        vehicle.LicensePlateNumber.Should().Be("ABC-123");
        vehicle.Make.Should().Be("Toyota");
        vehicle.Model.Should().Be("Corolla");
        vehicle.Vin.Should().Be(ValidVin);
        vehicle.Year.Should().Be(2020);
        vehicle.UserVehicleEvents.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidLicensePlate_ThrowsArgumentException(string? licensePlate)
    {
        // Act
        var act = () => UserVehicle.Create(UserId, licensePlate!, "Toyota", "Corolla", ValidVin, 2020);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithInvalidVinLength_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => UserVehicle.Create(UserId, "ABC-123", "Toyota", "Corolla", "SHORTVIN", 2020);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_WithYearTooFarInFuture_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidYear = DateTime.UtcNow.Year + 2;

        // Act
        var act = () => UserVehicle.Create(UserId, "ABC-123", "Toyota", "Corolla", ValidVin, invalidYear);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ChangeLicensePlateNumber_WithValidValue_TrimsAndUpdates()
    {
        // Arrange
        var vehicle = UserVehicle.Create(UserId, "ABC-123", "Toyota", "Corolla", ValidVin, 2020);

        // Act
        vehicle.ChangeLicensePlateNumber("  XYZ-999  ");

        // Assert
        vehicle.LicensePlateNumber.Should().Be("XYZ-999");
    }

    [Fact]
    public void ChangeLicensePlateNumber_WithEmptyValue_ThrowsArgumentException()
    {
        // Arrange
        var vehicle = UserVehicle.Create(UserId, "ABC-123", "Toyota", "Corolla", ValidVin, 2020);

        // Act
        var act = () => vehicle.ChangeLicensePlateNumber(" ");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddEvent_AddsEventToCollection()
    {
        // Arrange
        var vehicle = UserVehicle.Create(UserId, "ABC-123", "Toyota", "Corolla", ValidVin, 2020);

        // Act
        var addedEvent = vehicle.AddEvent("Oil Change", "Full synthetic oil change", DateTime.UtcNow);

        // Assert
        vehicle.UserVehicleEvents.Should().ContainSingle();
        vehicle.UserVehicleEvents.Single().Should().BeSameAs(addedEvent);
        addedEvent.UserVehicleId.Should().Be(vehicle.Id);
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
        addedEvent.Name.Should().Be("Tire Rotation");
        addedEvent.Description.Should().Be("Rotated all four tires");
        addedEvent.EventDate.Should().Be(newDate);
    }

    [Fact]
    public void UpdateEvent_WithUnknownEventId_ThrowsInvalidOperationException()
    {
        // Arrange
        var vehicle = UserVehicle.Create(UserId, "ABC-123", "Toyota", "Corolla", ValidVin, 2020);

        // Act
        var act = () => vehicle.UpdateEvent(Guid.NewGuid(), "Tire Rotation", "Rotated all four tires", DateTime.UtcNow);

        // Assert
        act.Should().Throw<InvalidOperationException>();
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
        vehicle.UserVehicleEvents.Should().BeEmpty();
    }

    [Fact]
    public void RemoveEvent_WithUnknownEventId_ThrowsInvalidOperationException()
    {
        // Arrange
        var vehicle = UserVehicle.Create(UserId, "ABC-123", "Toyota", "Corolla", ValidVin, 2020);

        // Act
        var act = () => vehicle.RemoveEvent(Guid.NewGuid());

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }
}
