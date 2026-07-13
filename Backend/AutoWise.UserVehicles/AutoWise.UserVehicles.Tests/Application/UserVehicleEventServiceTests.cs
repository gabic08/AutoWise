using AutoWise.CommonUtilities.Exceptions;
using AutoWise.UserVehicles.Application.Features.UserVehicles.Dtos;
using AutoWise.UserVehicles.Application.Features.UserVehicles.Services;
using AutoWise.UserVehicles.Domain.Models;
using AutoWise.UserVehicles.Tests.TestDoubles;

namespace AutoWise.UserVehicles.Tests.Application;

public class UserVehicleEventServiceTests
{
    private const string ValidVin = "1HGCM82633A004352";

    private static async Task<UserVehicle> SeedVehicleAsync(InMemoryUserVehiclesDbContext dbContext)
    {
        var vehicle = UserVehicle.Create(Guid.NewGuid(), "ABC-123", "Toyota", "Corolla", ValidVin, 2020);
        dbContext.UserVehicles.Add(vehicle);
        await dbContext.SaveChangesAsync();

        return vehicle;
    }

    [Fact]
    public async Task AddEventAsync_WithExistingVehicle_PersistsEventAndReturnsId()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var vehicle = await SeedVehicleAsync(dbContext);
        var sut = new UserVehicleEventService(dbContext);
        var request = new CreateUserVehicleEventRequest("Oil Change", "Full synthetic", DateTime.UtcNow);

        // Act
        var eventId = await sut.AddEventAsync(vehicle.Id, request);

        // Assert
        var persistedEvent = await dbContext.UserVehicleEvents.FindAsync(eventId);
        Assert.NotNull(persistedEvent);
        Assert.Equal("Oil Change", persistedEvent!.Name);
        Assert.Equal(vehicle.Id, persistedEvent.UserVehicleId);
    }

    [Fact]
    public async Task AddEventAsync_WithUnknownVehicle_ThrowsNotFoundException()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var sut = new UserVehicleEventService(dbContext);
        var request = new CreateUserVehicleEventRequest("Oil Change", "Full synthetic", DateTime.UtcNow);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => sut.AddEventAsync(Guid.NewGuid(), request));
    }

    [Fact]
    public async Task GetEventAsync_WithExistingEvent_ReturnsResponse()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var vehicle = await SeedVehicleAsync(dbContext);
        var sut = new UserVehicleEventService(dbContext);
        var eventId = await sut.AddEventAsync(vehicle.Id, new CreateUserVehicleEventRequest("Oil Change", "Full synthetic", DateTime.UtcNow));

        // Act
        var response = await sut.GetEventAsync(vehicle.Id, eventId);

        // Assert
        Assert.Equal(eventId, response.Id);
        Assert.Equal("Oil Change", response.Name);
    }

    [Fact]
    public async Task GetEventAsync_WithUnknownEvent_ThrowsNotFoundException()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var vehicle = await SeedVehicleAsync(dbContext);
        var sut = new UserVehicleEventService(dbContext);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => sut.GetEventAsync(vehicle.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateEventAsync_WithExistingEvent_UpdatesFields()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var vehicle = await SeedVehicleAsync(dbContext);
        var sut = new UserVehicleEventService(dbContext);
        var eventId = await sut.AddEventAsync(vehicle.Id, new CreateUserVehicleEventRequest("Oil Change", "Full synthetic", DateTime.UtcNow));
        var newDate = DateTime.UtcNow.AddDays(1);

        // Act
        await sut.UpdateEventAsync(vehicle.Id, eventId, new UpdateUserVehicleEventRequest("Tire Rotation", "Rotated tires", newDate));

        // Assert
        var updatedEvent = await dbContext.UserVehicleEvents.FindAsync(eventId);
        Assert.Equal("Tire Rotation", updatedEvent!.Name);
        Assert.Equal("Rotated tires", updatedEvent.Description);
        Assert.Equal(newDate, updatedEvent.EventDate);
    }

    [Fact]
    public async Task UpdateEventAsync_WithUnknownVehicle_ThrowsNotFoundException()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var sut = new UserVehicleEventService(dbContext);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.UpdateEventAsync(Guid.NewGuid(), Guid.NewGuid(), new UpdateUserVehicleEventRequest("Tire Rotation", "Rotated tires", DateTime.UtcNow)));
    }

    [Fact]
    public async Task UpdateEventAsync_WithUnknownEvent_ThrowsNotFoundException()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var vehicle = await SeedVehicleAsync(dbContext);
        var sut = new UserVehicleEventService(dbContext);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.UpdateEventAsync(vehicle.Id, Guid.NewGuid(), new UpdateUserVehicleEventRequest("Tire Rotation", "Rotated tires", DateTime.UtcNow)));
    }

    [Fact]
    public async Task RemoveEventAsync_WithExistingEvent_RemovesEvent()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var vehicle = await SeedVehicleAsync(dbContext);
        var sut = new UserVehicleEventService(dbContext);
        var eventId = await sut.AddEventAsync(vehicle.Id, new CreateUserVehicleEventRequest("Oil Change", "Full synthetic", DateTime.UtcNow));

        // Act
        await sut.RemoveEventAsync(vehicle.Id, eventId);

        // Assert
        Assert.Null(await dbContext.UserVehicleEvents.FindAsync(eventId));
    }

    [Fact]
    public async Task RemoveEventAsync_WithUnknownEvent_ThrowsNotFoundException()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var vehicle = await SeedVehicleAsync(dbContext);
        var sut = new UserVehicleEventService(dbContext);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => sut.RemoveEventAsync(vehicle.Id, Guid.NewGuid()));
    }
}
