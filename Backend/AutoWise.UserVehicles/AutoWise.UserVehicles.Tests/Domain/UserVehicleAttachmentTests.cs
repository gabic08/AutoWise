using AutoWise.UserVehicles.Domain.Models;
using FluentAssertions;

namespace AutoWise.UserVehicles.Tests.Domain;

public class UserVehicleAttachmentTests
{
    private static readonly Guid UserVehicleId = Guid.NewGuid();
    private static readonly Guid MediaAttachmentId = Guid.NewGuid();
    private const string OriginalFileName = "invoice.pdf";
    private const string ContentType = "application/pdf";
    private const long SizeInBytes = 2048;

    [Fact]
    public void Create_WithValidData_SetsAllProperties()
    {
        // Act
        var attachment = UserVehicleAttachment.Create(UserVehicleId, MediaAttachmentId, OriginalFileName, ContentType, SizeInBytes);

        // Assert
        attachment.UserVehicleId.Should().Be(UserVehicleId);
        attachment.MediaAttachmentId.Should().Be(MediaAttachmentId);
        attachment.OriginalFileName.Should().Be(OriginalFileName);
        attachment.ContentType.Should().Be(ContentType);
        attachment.SizeInBytes.Should().Be(SizeInBytes);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidOriginalFileName_ThrowsArgumentException(string? originalFileName)
    {
        // Act
        var act = () => UserVehicleAttachment.Create(UserVehicleId, MediaAttachmentId, originalFileName!, ContentType, SizeInBytes);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidContentType_ThrowsArgumentException(string? contentType)
    {
        // Act
        var act = () => UserVehicleAttachment.Create(UserVehicleId, MediaAttachmentId, OriginalFileName, contentType!, SizeInBytes);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositiveSize_ThrowsArgumentOutOfRangeException(long sizeInBytes)
    {
        // Act
        var act = () => UserVehicleAttachment.Create(UserVehicleId, MediaAttachmentId, OriginalFileName, ContentType, sizeInBytes);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
