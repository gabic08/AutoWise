using AutoWise.CommonUtilities.Exceptions;
using AutoWise.UserVehicles.Application.Features.UserVehicles.Dtos;
using AutoWise.UserVehicles.Application.Features.UserVehicles.Interfaces;
using AutoWise.UserVehicles.Application.Features.UserVehicles.Services;
using AutoWise.UserVehicles.Domain.Models;
using AutoWise.UserVehicles.Tests.TestDoubles;
using NSubstitute;

namespace AutoWise.UserVehicles.Tests.Application;

public class UserVehiclesServiceTests
{
    private const string ValidVin = "1HGCM82633A004352";

    private static IVehicleSpecificationsService CreateSpecsService(string make = "Toyota", string model = "Corolla", string year = "2020")
    {
        var specsService = Substitute.For<IVehicleSpecificationsService>();
        specsService.GetSpecificationsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(
            [
                new VehicleSpecificationDto("Make", make),
                new VehicleSpecificationDto("Model", model),
                new VehicleSpecificationDto("Model Year", year)
            ]);

        return specsService;
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_PersistsVehicleAndReturnsId()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var sut = new UserVehiclesService(dbContext, CreateSpecsService());
        var request = new CreateUserVehicleRequest(ValidVin, "ABC-123");
        var userId = Guid.NewGuid();

        // Act
        var vehicleId = await sut.CreateAsync(request, userId);

        // Assert
        var persisted = await dbContext.UserVehicles.FindAsync(vehicleId);
        Assert.NotNull(persisted);
        Assert.Equal("ABC-123", persisted!.LicensePlateNumber);
        Assert.Equal("Toyota", persisted.Make);
        Assert.Equal("Corolla", persisted.Model);
        Assert.Equal(2020, persisted.Year);
        Assert.Equal(userId, persisted.UserId);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsResponse()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var vehicle = UserVehicle.Create(Guid.NewGuid(), "ABC-123", "Toyota", "Corolla", ValidVin, 2020);
        dbContext.UserVehicles.Add(vehicle);
        await dbContext.SaveChangesAsync();
        var sut = new UserVehiclesService(dbContext, CreateSpecsService());

        // Act
        var response = await sut.GetByIdAsync(vehicle.Id);

        // Assert
        Assert.Equal(vehicle.Id, response.Id);
        Assert.Equal("ABC-123", response.LicensePlateNumber);
    }

    [Fact]
    public async Task GetByIdAsync_WithUnknownId_ThrowsNotFoundException()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var sut = new UserVehiclesService(dbContext, CreateSpecsService());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => sut.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateAsync_WithExistingId_UpdatesLicensePlate()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var vehicle = UserVehicle.Create(Guid.NewGuid(), "ABC-123", "Toyota", "Corolla", ValidVin, 2020);
        dbContext.UserVehicles.Add(vehicle);
        await dbContext.SaveChangesAsync();
        var sut = new UserVehiclesService(dbContext, CreateSpecsService());

        // Act
        await sut.UpdateAsync(vehicle.Id, new UpdateUserVehicleRequest("XYZ-999"));

        // Assert
        var updated = await dbContext.UserVehicles.FindAsync(vehicle.Id);
        Assert.Equal("XYZ-999", updated!.LicensePlateNumber);
    }

    [Fact]
    public async Task UpdateAsync_WithUnknownId_ThrowsNotFoundException()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var sut = new UserVehiclesService(dbContext, CreateSpecsService());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.UpdateAsync(Guid.NewGuid(), new UpdateUserVehicleRequest("XYZ-999")));
    }

    [Fact]
    public async Task DeleteAsync_WithExistingId_RemovesVehicle()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var vehicle = UserVehicle.Create(Guid.NewGuid(), "ABC-123", "Toyota", "Corolla", ValidVin, 2020);
        dbContext.UserVehicles.Add(vehicle);
        await dbContext.SaveChangesAsync();
        var sut = new UserVehiclesService(dbContext, CreateSpecsService());

        // Act
        await sut.DeleteAsync(vehicle.Id);

        // Assert
        Assert.Null(await dbContext.UserVehicles.FindAsync(vehicle.Id));
    }

    [Fact]
    public async Task DeleteAsync_WithUnknownId_ThrowsNotFoundException()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var sut = new UserVehiclesService(dbContext, CreateSpecsService());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => sut.DeleteAsync(Guid.NewGuid()));
    }
}
