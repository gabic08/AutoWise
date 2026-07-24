namespace AutoWise.Users.Tests.Domain;

public class UserTests
{
    private const string Email = "a@test.com";
    private const string Provider = "AzureAD";
    private const string DisplayName = "John Doe";
    private const string ExternalId = "1234";

    [Fact]
    public void Create_WithValidData_SetsAllProperties()
    {
        // Act
        var user = User.Create(DisplayName, Email, ExternalId, Provider);

        // Assert
        user.DisplayName.Should().Be(DisplayName);
        user.Email.Should().Be(Email);
        user.Provider.Should().Be(Provider);
        user.ExternalId.Should().Be(ExternalId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithInvalidEmail_ThowsArgumentException(string email)
    {
        // Act
        var act = () => User.Create(DisplayName, email, ExternalId, Provider);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithInvalidProvider_ThowsArgumentException(string provider)
    {
        // Act
        var act = () => User.Create(DisplayName, Email, ExternalId, provider);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithInvalidExternalId_ThowsArgumentException(string externalId)
    {
        // Act
        var act = () => User.Create(DisplayName, Email, externalId, Provider);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithInvalidDisplayName_ThrowsArgumentException(string displayName)
    {
        // Act
        var act = () => User.Create(displayName, Email, ExternalId, Provider);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateProfile_WithValidData_UpdatesEmailAndDisplayName()
    {
        // Arrange
        var user = User.Create(DisplayName, Email, ExternalId, Provider);
        const string updatedEmail = "updated@test.com";
        const string updatedDisplayName = "Jane Doe";

        // Act
        user.UpdateProfile(updatedEmail, updatedDisplayName);

        // Assert
        user.Email.Should().Be(updatedEmail);
        user.DisplayName.Should().Be(updatedDisplayName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void UpdateProfile_WithInvalidEmail_ThrowsArgumentException(string email)
    {
        // Arrange
        var user = User.Create(DisplayName, Email, ExternalId, Provider);

        // Act
        var act = () => user.UpdateProfile(email, DisplayName);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void UpdateProfile_WithInvalidDisplayName_ThrowsArgumentException(string displayName)
    {
        // Arrange
        var user = User.Create(DisplayName, Email, ExternalId, Provider);

        // Act
        var act = () => user.UpdateProfile(Email, displayName);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
