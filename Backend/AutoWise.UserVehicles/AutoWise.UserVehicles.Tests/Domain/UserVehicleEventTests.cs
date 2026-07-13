using AutoWise.UserVehicles.Domain.Models;

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
        Assert.Equal(vehicleId, vehicleEvent.UserVehicleId);
        Assert.Equal("Oil Change", vehicleEvent.Name);
        Assert.Equal("Full synthetic oil change", vehicleEvent.Description);
        Assert.Equal(eventDate, vehicleEvent.EventDate);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidName_ThrowsArgumentException(string? name)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            UserVehicleEvent.Create(Guid.NewGuid(), name!, "Description", DateTime.UtcNow));
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
        Assert.Equal("Tire Rotation", vehicleEvent.Name);
        Assert.Equal("Rotated all four tires", vehicleEvent.Description);
        Assert.Equal(newDate, vehicleEvent.EventDate);
    }

    [Fact]
    public void Update_WithInvalidName_ThrowsArgumentException()
    {
        // Arrange
        var vehicleEvent = UserVehicleEvent.Create(Guid.NewGuid(), "Oil Change", "Full synthetic", DateTime.UtcNow);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => vehicleEvent.Update(" ", "Description", DateTime.UtcNow));
    }
}
