using AutoWise.Media.Domain.Models;
using FluentAssertions;

namespace AutoWise.Media.Tests.Domain;

public class MediaAttachmentTests
{
    private static readonly Guid MediaFileId = Guid.NewGuid();
    private static readonly Guid ParentEntityId = Guid.NewGuid();
    private const string ParentType = "UserVehicle";
    private const string OriginalFileName = "invoice.pdf";

    [Fact]
    public void Create_WithValidData_SetsAllProperties()
    {
        // Act
        var attachment = MediaAttachment.Create(MediaFileId, ParentType, ParentEntityId, OriginalFileName);

        // Assert
        attachment.MediaFileId.Should().Be(MediaFileId);
        attachment.ParentType.Should().Be(ParentType);
        attachment.ParentEntityId.Should().Be(ParentEntityId);
        attachment.OriginalFileName.Should().Be(OriginalFileName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidParentType_ThrowsArgumentException(string? parentType)
    {
        // Act
        var act = () => MediaAttachment.Create(MediaFileId, parentType!, ParentEntityId, OriginalFileName);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyParentEntityId_ThrowsArgumentException()
    {
        // Act
        var act = () => MediaAttachment.Create(MediaFileId, ParentType, Guid.Empty, OriginalFileName);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidOriginalFileName_ThrowsArgumentException(string? originalFileName)
    {
        // Act
        var act = () => MediaAttachment.Create(MediaFileId, ParentType, ParentEntityId, originalFileName!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
