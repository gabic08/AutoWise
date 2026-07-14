using AutoWise.CommonUtilities.Exceptions;
using AutoWise.UserVehicles.Application.Features.UserVehicles.Dtos;
using AutoWise.UserVehicles.Application.Features.UserVehicles.Interfaces;
using AutoWise.UserVehicles.Application.Features.UserVehicles.Services;
using AutoWise.UserVehicles.Domain.Models;
using AutoWise.UserVehicles.Tests.TestDoubles;
using FluentAssertions;
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
        persisted.Should().NotBeNull();
        persisted!.LicensePlateNumber.Should().Be("ABC-123");
        persisted.Make.Should().Be("Toyota");
        persisted.Model.Should().Be("Corolla");
        persisted.Year.Should().Be(2020);
        persisted.UserId.Should().Be(userId);
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
        response.Id.Should().Be(vehicle.Id);
        response.LicensePlateNumber.Should().Be("ABC-123");
    }

    [Fact]
    public async Task GetByIdAsync_WithUnknownId_ThrowsNotFoundException()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var sut = new UserVehiclesService(dbContext, CreateSpecsService());

        // Act
        Func<Task> act = () => sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
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
        updated!.LicensePlateNumber.Should().Be("XYZ-999");
    }

    [Fact]
    public async Task UpdateAsync_WithUnknownId_ThrowsNotFoundException()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var sut = new UserVehiclesService(dbContext, CreateSpecsService());

        // Act
        Func<Task> act = () => sut.UpdateAsync(Guid.NewGuid(), new UpdateUserVehicleRequest("XYZ-999"));

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
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
        var deleted = await dbContext.UserVehicles.FindAsync(vehicle.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithUnknownId_ThrowsNotFoundException()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var sut = new UserVehiclesService(dbContext, CreateSpecsService());

        // Act
        Func<Task> act = () => sut.DeleteAsync(Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
