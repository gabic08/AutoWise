using AutoWise.UserVehicles.Domain.Models;
using FluentAssertions;

namespace AutoWise.UserVehicles.Tests.Domain;

public class UserVehicleEventTests
{
    [Fact]
    public void Create_WithValidData_SetsAllProperties()
    {
        // Arrange
        var vehicleId = Guid.NewGuid();
        var eventDate = DateTime.UtcNow;

        // Act
        var vehicleEvent = UserVehicleEvent.Create(vehicleId, "Oil Change", "Full synthetic oil change", eventDate);

        // Assert
        vehicleEvent.UserVehicleId.Should().Be(vehicleId);
        vehicleEvent.Name.Should().Be("Oil Change");
        vehicleEvent.Description.Should().Be("Full synthetic oil change");
        vehicleEvent.EventDate.Should().Be(eventDate);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidName_ThrowsArgumentException(string? name)
    {
        // Act
        var act = () => UserVehicleEvent.Create(Guid.NewGuid(), name!, "Description", DateTime.UtcNow);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Update_WithValidData_UpdatesFields()
    {
        // Arrange
        var vehicleEvent = UserVehicleEvent.Create(Guid.NewGuid(), "Oil Change", "Full synthetic", DateTime.UtcNow);
        var newDate = DateTime.UtcNow.AddDays(1);

        // Act
        vehicleEvent.Update("Tire Rotation", "Rotated all four tires", newDate);

        // Assert
        vehicleEvent.Name.Should().Be("Tire Rotation");
        vehicleEvent.Description.Should().Be("Rotated all four tires");
        vehicleEvent.EventDate.Should().Be(newDate);
    }

    [Fact]
    public void Update_WithInvalidName_ThrowsArgumentException()
    {
        // Arrange
        var vehicleEvent = UserVehicleEvent.Create(Guid.NewGuid(), "Oil Change", "Full synthetic", DateTime.UtcNow);

        // Act
        var act = () => vehicleEvent.Update(" ", "Description", DateTime.UtcNow);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
