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

    private static IDistributedCache CreateCache() => Substitute.For<IDistributedCache>();

    private static IDistributedCache CreateCacheReturning(string vin, string make, string model, string year)
    {
        var cache = Substitute.For<IDistributedCache>();
        var cachedSpecifications = JsonSerializer.Serialize(new List<VehicleSpecificationDto>
        {
            new("Make", make),
            new("Model", model),
            new("Model Year", year)
        });

        cache.GetAsync($"vehicle-specifications:{vin}", Arg.Any<CancellationToken>())
            .Returns(Encoding.UTF8.GetBytes(cachedSpecifications));

        return cache;
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_PersistsVehicleAndReturnsId()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var sut = new UserVehiclesService(dbContext, CreateSpecsService(), CreateCache());
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
    public async Task CreateAsync_WithCachedSpecifications_UsesCacheAndSkipsGrpcCall()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var specsService = CreateSpecsService();
        var cache = CreateCacheReturning(ValidVin, "Honda", "Civic", "2019");
        var sut = new UserVehiclesService(dbContext, specsService, cache);
        var request = new CreateUserVehicleRequest(ValidVin, "ABC-123");
        var userId = Guid.NewGuid();

        // Act
        var vehicleId = await sut.CreateAsync(request, userId);

        // Assert
        var persisted = await dbContext.UserVehicles.FindAsync(vehicleId);
        persisted!.Make.Should().Be("Honda");
        persisted.Model.Should().Be("Civic");
        persisted.Year.Should().Be(2019);
        await specsService.DidNotReceive().GetSpecificationsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAllForUserAsync_WithExistingVehicles_ReturnsOnlyVehiclesForThatUser()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var userId = Guid.NewGuid();
        var ownVehicle = UserVehicle.Create(userId, "ABC-123", "Toyota", "Corolla", ValidVin, 2020);
        var otherUsersVehicle = UserVehicle.Create(Guid.NewGuid(), "XYZ-999", "Honda", "Civic", "2HGES16585H543212", 2019);
        dbContext.UserVehicles.AddRange(ownVehicle, otherUsersVehicle);
        await dbContext.SaveChangesAsync();
        var sut = new UserVehiclesService(dbContext, CreateSpecsService(), CreateCache());

        // Act
        var response = await sut.GetAllForUserAsync(userId, new GetUserVehiclesRequest());

        // Assert
        response.Entities.Should().ContainSingle(v => v.Id == ownVehicle.Id);
        response.PagedQueryResponse.TotalItemsCount.Should().Be(1);
    }

    [Fact]
    public async Task GetAllForUserAsync_WithPagination_ReturnsRequestedPageAndMetadata()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var userId = Guid.NewGuid();
        var vins = new[] { "1HGCM82633A004352", "2HGES16585H543212", "JH4KA7561PC008269", "1FTRW07L83KB78041", "5YJSA1E26FF101183" };
        foreach (var vin in vins)
        {
            dbContext.UserVehicles.Add(UserVehicle.Create(userId, $"PLT-{vin[..3]}", "Toyota", "Corolla", vin, 2020));
        }
        await dbContext.SaveChangesAsync();
        var sut = new UserVehiclesService(dbContext, CreateSpecsService(), CreateCache());

        // Act
        var response = await sut.GetAllForUserAsync(userId, new GetUserVehiclesRequest(Page: 2, PageSize: 2));

        // Assert
        response.Entities.Should().HaveCount(2);
        response.PagedQueryResponse.CurrentPage.Should().Be(2);
        response.PagedQueryResponse.PageSize.Should().Be(2);
        response.PagedQueryResponse.TotalItemsCount.Should().Be(5);
        response.PagedQueryResponse.TotalPagesCount.Should().Be(3);
    }

    [Fact]
    public async Task GetAllForUserAsync_WithSearch_ReturnsOnlyMatchingVehicles()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var userId = Guid.NewGuid();
        var corolla = UserVehicle.Create(userId, "ABC-123", "Toyota", "Corolla", ValidVin, 2020);
        var civic = UserVehicle.Create(userId, "XYZ-999", "Honda", "Civic", "2HGES16585H543212", 2019);
        dbContext.UserVehicles.AddRange(corolla, civic);
        await dbContext.SaveChangesAsync();
        var sut = new UserVehiclesService(dbContext, CreateSpecsService(), CreateCache());

        // Act
        var response = await sut.GetAllForUserAsync(userId, new GetUserVehiclesRequest(Search: "corolla"));

        // Assert
        response.Entities.Should().ContainSingle(v => v.Id == corolla.Id);
    }

    [Fact]
    public async Task GetAllForUserAsync_WithSortByYearDescending_ReturnsVehiclesInDescendingYearOrder()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var userId = Guid.NewGuid();
        var older = UserVehicle.Create(userId, "ABC-123", "Toyota", "Corolla", ValidVin, 2018);
        var newer = UserVehicle.Create(userId, "XYZ-999", "Honda", "Civic", "2HGES16585H543212", 2022);
        dbContext.UserVehicles.AddRange(older, newer);
        await dbContext.SaveChangesAsync();
        var sut = new UserVehiclesService(dbContext, CreateSpecsService(), CreateCache());

        // Act
        var response = await sut.GetAllForUserAsync(userId, new GetUserVehiclesRequest(SortBy: "year", SortDescending: true));

        // Assert
        response.Entities.Select(v => v.Id).Should().ContainInOrder(newer.Id, older.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsResponse()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var userId = Guid.NewGuid();
        var vehicle = UserVehicle.Create(userId, "ABC-123", "Toyota", "Corolla", ValidVin, 2020);
        dbContext.UserVehicles.Add(vehicle);
        await dbContext.SaveChangesAsync();
        var sut = new UserVehiclesService(dbContext, CreateSpecsService(), CreateCache());

        // Act
        var response = await sut.GetByIdAsync(vehicle.Id, userId);

        // Assert
        response.Id.Should().Be(vehicle.Id);
        response.LicensePlateNumber.Should().Be("ABC-123");
    }

    [Fact]
    public async Task GetByIdAsync_WithUnknownId_ThrowsNotFoundException()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var sut = new UserVehiclesService(dbContext, CreateSpecsService(), CreateCache());

        // Act
        Func<Task> act = () => sut.GetByIdAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetByIdAsync_BelongingToAnotherUser_ThrowsNotFoundException()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var vehicle = UserVehicle.Create(Guid.NewGuid(), "ABC-123", "Toyota", "Corolla", ValidVin, 2020);
        dbContext.UserVehicles.Add(vehicle);
        await dbContext.SaveChangesAsync();
        var sut = new UserVehiclesService(dbContext, CreateSpecsService(), CreateCache());

        // Act
        Func<Task> act = () => sut.GetByIdAsync(vehicle.Id, Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_WithExistingId_UpdatesLicensePlate()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var userId = Guid.NewGuid();
        var vehicle = UserVehicle.Create(userId, "ABC-123", "Toyota", "Corolla", ValidVin, 2020);
        dbContext.UserVehicles.Add(vehicle);
        await dbContext.SaveChangesAsync();
        var sut = new UserVehiclesService(dbContext, CreateSpecsService(), CreateCache());

        // Act
        await sut.UpdateAsync(vehicle.Id, new UpdateUserVehicleRequest("XYZ-999"), userId);

        // Assert
        var updated = await dbContext.UserVehicles.FindAsync(vehicle.Id);
        updated!.LicensePlateNumber.Should().Be("XYZ-999");
    }

    [Fact]
    public async Task UpdateAsync_WithUnknownId_ThrowsNotFoundException()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var sut = new UserVehiclesService(dbContext, CreateSpecsService(), CreateCache());

        // Act
        Func<Task> act = () => sut.UpdateAsync(Guid.NewGuid(), new UpdateUserVehicleRequest("XYZ-999"), Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_BelongingToAnotherUser_ThrowsNotFoundException()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var vehicle = UserVehicle.Create(Guid.NewGuid(), "ABC-123", "Toyota", "Corolla", ValidVin, 2020);
        dbContext.UserVehicles.Add(vehicle);
        await dbContext.SaveChangesAsync();
        var sut = new UserVehiclesService(dbContext, CreateSpecsService(), CreateCache());

        // Act
        Func<Task> act = () => sut.UpdateAsync(vehicle.Id, new UpdateUserVehicleRequest("XYZ-999"), Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_WithExistingId_RemovesVehicle()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var userId = Guid.NewGuid();
        var vehicle = UserVehicle.Create(userId, "ABC-123", "Toyota", "Corolla", ValidVin, 2020);
        dbContext.UserVehicles.Add(vehicle);
        await dbContext.SaveChangesAsync();
        var sut = new UserVehiclesService(dbContext, CreateSpecsService(), CreateCache());

        // Act
        await sut.DeleteAsync(vehicle.Id, userId);

        // Assert
        var deleted = await dbContext.UserVehicles.FindAsync(vehicle.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithUnknownId_ThrowsNotFoundException()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var sut = new UserVehiclesService(dbContext, CreateSpecsService(), CreateCache());

        // Act
        Func<Task> act = () => sut.DeleteAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_BelongingToAnotherUser_ThrowsNotFoundException()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var vehicle = UserVehicle.Create(Guid.NewGuid(), "ABC-123", "Toyota", "Corolla", ValidVin, 2020);
        dbContext.UserVehicles.Add(vehicle);
        await dbContext.SaveChangesAsync();
        var sut = new UserVehiclesService(dbContext, CreateSpecsService(), CreateCache());

        // Act
        Func<Task> act = () => sut.DeleteAsync(vehicle.Id, Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
