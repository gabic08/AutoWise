namespace AutoWise.Users.Tests.Application;

public class UserServiceTests
{
    private const string Email = "a@test.com";
    private const string Provider = "AzureAD";
    private const string DisplayName = "John Doe";
    private const string ExternalId = "1234";

    [Fact]
    public async Task CreateOrSyncUserAsync_WithValidRequest_ShouldCreateNewUser()
    {
        // Arrange
        await using var dbContext = InMemoryUsersDbContext.Create();
        var sut = new UserService(dbContext);
        var request = new CreateOrSyncUserRequest(ExternalId, Provider, Email, DisplayName);
        var existingUser = await dbContext.Users.FirstOrDefaultAsync
            (u => u.Provider == request.Provider && u.ExternalId == request.ExternalId);

        // Act
        var syncUserResponse = await sut.CreateOrSyncUserAsync(request);

        // Assert
        existingUser.Should().Be(null);
        syncUserResponse.Should().NotBeNull();
        syncUserResponse.Email.Should().Be(Email);
        syncUserResponse.Provider.Should().Be(Provider);
        syncUserResponse.DisplayName.Should().Be(DisplayName);
        syncUserResponse.ExternalId.Should().Be(ExternalId);
        syncUserResponse.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateOrSyncUserAsync_WithValidRequest_ShouldUpdateExistingUser()
    {
        // Arrange
        await using var dbContext = InMemoryUsersDbContext.Create();
        var sut = new UserService(dbContext);

        var newUser = User.Create(DisplayName, Email, ExternalId, Provider);
        await dbContext.AddAsync(newUser);
        await dbContext.SaveChangesAsync();
        var existingUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Provider == Provider && u.ExternalId == ExternalId);

        var updatedEmailValue = "b@test.com";
        var updatedDisplayNameValue = "DisplayName updated";
        var request = new CreateOrSyncUserRequest(ExternalId, Provider, updatedEmailValue, updatedDisplayNameValue);

        // Act
        var syncUserResponse = await sut.CreateOrSyncUserAsync(request);

        // Assert
        syncUserResponse.Should().NotBeNull();
        syncUserResponse.Email.Should().Be(updatedEmailValue);
        syncUserResponse.Provider.Should().Be(Provider);
        syncUserResponse.DisplayName.Should().Be(updatedDisplayNameValue);
        syncUserResponse.ExternalId.Should().Be(ExternalId);
        syncUserResponse.Id.Should().Be(existingUser.Id);
    }
}
