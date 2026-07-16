using AutoWise.Media.Domain.Enums;
using AutoWise.Media.Domain.Models;
using FluentAssertions;

namespace AutoWise.Media.Tests.Domain;

public class MediaFileTests
{
    private const string ValidHash = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b85";
    private const string ValidContentType = "image/png";
    private const string ValidExtension = "png";
    private const string ValidStorageKey = "e3/b0/e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b85.png";

    [Fact]
    public void Create_WithValidData_SetsAllPropertiesAndGeneratesId()
    {
        // Act
        var mediaFile = MediaFile.Create(ValidHash, ValidContentType, ValidExtension, 1024, MediaStorageProvider.LocalDisk, ValidStorageKey);

        // Assert
        mediaFile.Id.Should().NotBeEmpty();
        mediaFile.ContentHash.Should().Be(ValidHash);
        mediaFile.ContentType.Should().Be(ValidContentType);
        mediaFile.FileExtension.Should().Be(ValidExtension);
        mediaFile.SizeInBytes.Should().Be(1024);
        mediaFile.StorageProvider.Should().Be(MediaStorageProvider.LocalDisk);
        mediaFile.StorageKey.Should().Be(ValidStorageKey);
        mediaFile.MediaAttachments.Should().BeEmpty();
    }

    [Fact]
    public void Create_NormalizesContentHashToLowercase()
    {
        // Act
        var mediaFile = MediaFile.Create(ValidHash.ToUpperInvariant(), ValidContentType, ValidExtension, 1024, MediaStorageProvider.LocalDisk, ValidStorageKey);

        // Assert
        mediaFile.ContentHash.Should().Be(ValidHash);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidContentHash_ThrowsArgumentException(string? contentHash)
    {
        // Act
        var act = () => MediaFile.Create(contentHash!, ValidContentType, ValidExtension, 1024, MediaStorageProvider.LocalDisk, ValidStorageKey);

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
        var act = () => MediaFile.Create(ValidHash, contentType!, ValidExtension, 1024, MediaStorageProvider.LocalDisk, ValidStorageKey);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidFileExtension_ThrowsArgumentException(string? fileExtension)
    {
        // Act
        var act = () => MediaFile.Create(ValidHash, ValidContentType, fileExtension!, 1024, MediaStorageProvider.LocalDisk, ValidStorageKey);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_StripsLeadingDotAndLowercasesFileExtension()
    {
        // Act
        var mediaFile = MediaFile.Create(ValidHash, ValidContentType, ".PNG", 1024, MediaStorageProvider.LocalDisk, ValidStorageKey);

        // Assert
        mediaFile.FileExtension.Should().Be("png");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositiveSize_ThrowsArgumentOutOfRangeException(long sizeInBytes)
    {
        // Act
        var act = () => MediaFile.Create(ValidHash, ValidContentType, ValidExtension, sizeInBytes, MediaStorageProvider.LocalDisk, ValidStorageKey);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidStorageKey_ThrowsArgumentException(string? storageKey)
    {
        // Act
        var act = () => MediaFile.Create(ValidHash, ValidContentType, ValidExtension, 1024, MediaStorageProvider.LocalDisk, storageKey!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
