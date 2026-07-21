using AutoWise.CommonUtilities.Exceptions;
using AutoWise.CommonUtilities.Messaging.Abstractions;
using AutoWise.CommonUtilities.Messaging.Contracts.Media;
using AutoWise.Media.Application.Config;
using AutoWise.Media.Application.Dtos;
using AutoWise.Media.Application.Features.MediaAttachments;
using AutoWise.Media.Application.Storage;
using AutoWise.Media.Domain.Enums;
using AutoWise.Media.Tests.TestDoubles;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Text;

namespace AutoWise.Media.Tests.Application;

public class MediaAttachmentServiceTests
{
    private static readonly Guid ParentEntityId = Guid.NewGuid();
    private const string ParentType = "UserVehicle";

    private static IOptions<MediaUploadOptions> CreateUploadOptions(params string[] allowedMimeTypes)
    {
        return Options.Create(new MediaUploadOptions { AllowedMimeTypes = [.. allowedMimeTypes] });
    }

    private static (IFileStorageProviderResolver Resolver, IFileStorageProvider Provider) CreateStorage(
        MediaStorageProvider providerType = MediaStorageProvider.LocalDisk)
    {
        var provider = Substitute.For<IFileStorageProvider>();
        provider.ProviderType.Returns(providerType);

        var resolver = Substitute.For<IFileStorageProviderResolver>();
        resolver.ResolveActiveProvider().Returns(provider);
        resolver.Resolve(providerType).Returns(provider);

        return (resolver, provider);
    }

    private static UploadMediaRequest CreateRequest(string content = "hello world", string contentType = "text/plain", string fileName = "note.txt")
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        return new UploadMediaRequest(stream, contentType, fileName, ParentType, ParentEntityId);
    }

    private static IEventPublisher CreateEventPublisher()
    {
        return Substitute.For<IEventPublisher>();
    }

    [Fact]
    public async Task UploadAsync_WithDisallowedContentType_ThrowsBadRequestException()
    {
        // Arrange
        await using var dbContext = InMemoryMediaDbContext.Create();
        var (resolver, _) = CreateStorage();
        var eventPublisher = CreateEventPublisher();
        var sut = new MediaAttachmentService(dbContext, resolver, CreateUploadOptions("application/pdf"), eventPublisher);
        var request = CreateRequest(contentType: "image/png");

        // Act
        Func<Task> act = () => sut.UploadAsync(request);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task UploadAsync_WithNewContent_SavesToStorageAndPersistsMediaFileAndAttachment()
    {
        // Arrange
        await using var dbContext = InMemoryMediaDbContext.Create();
        var (resolver, provider) = CreateStorage();
        var eventPublisher = CreateEventPublisher();
        var sut = new MediaAttachmentService(dbContext, resolver, CreateUploadOptions("text/plain"), eventPublisher);
        var request = CreateRequest();

        // Act
        var attachmentId = await sut.UploadAsync(request);

        // Assert
        await provider.Received(1).SaveAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>());

        var attachment = await dbContext.MediaAttachments.FindAsync(attachmentId);
        attachment.Should().NotBeNull();
        attachment!.ParentType.Should().Be(ParentType);
        attachment.ParentEntityId.Should().Be(ParentEntityId);
        attachment.OriginalFileName.Should().Be("note.txt");

        var mediaFile = await dbContext.MediaFiles.FindAsync(attachment.MediaFileId);
        mediaFile.Should().NotBeNull();
        mediaFile!.ContentType.Should().Be("text/plain");
        mediaFile.StorageProvider.Should().Be(MediaStorageProvider.LocalDisk);

        await eventPublisher.Received(1).PublishAsync(
            Arg.Is<MediaAttachmentUploaded>(e =>
                e.MediaAttachmentId == attachmentId &&
                e.ParentType == ParentType &&
                e.ParentEntityId == ParentEntityId &&
                e.OriginalFileName == "note.txt" &&
                e.ContentType == "text/plain" &&
                e.SizeInBytes == mediaFile.SizeInBytes),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UploadAsync_WithDuplicateContent_ReusesExistingMediaFileAndSkipsStorageWrite()
    {
        // Arrange
        await using var dbContext = InMemoryMediaDbContext.Create();
        var (resolver, provider) = CreateStorage();
        var eventPublisher = CreateEventPublisher();
        var sut = new MediaAttachmentService(dbContext, resolver, CreateUploadOptions("text/plain"), eventPublisher);

        var firstAttachmentId = await sut.UploadAsync(CreateRequest(fileName: "first.txt"));

        // Act
        var secondAttachmentId = await sut.UploadAsync(CreateRequest(fileName: "second.txt"));

        // Assert
        await provider.Received(1).SaveAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>());

        var firstAttachment = await dbContext.MediaAttachments.FindAsync(firstAttachmentId);
        var secondAttachment = await dbContext.MediaAttachments.FindAsync(secondAttachmentId);

        secondAttachment!.MediaFileId.Should().Be(firstAttachment!.MediaFileId);
        (await dbContext.MediaFiles.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsResponse()
    {
        // Arrange
        await using var dbContext = InMemoryMediaDbContext.Create();
        var (resolver, _) = CreateStorage();
        var eventPublisher = CreateEventPublisher();
        var sut = new MediaAttachmentService(dbContext, resolver, CreateUploadOptions("text/plain"), eventPublisher);
        var attachmentId = await sut.UploadAsync(CreateRequest());

        // Act
        var response = await sut.GetByIdAsync(attachmentId);

        // Assert
        response.Id.Should().Be(attachmentId);
        response.OriginalFileName.Should().Be("note.txt");
        response.ContentType.Should().Be("text/plain");
        response.ParentType.Should().Be(ParentType);
        response.ParentEntityId.Should().Be(ParentEntityId);
    }

    [Fact]
    public async Task GetByIdAsync_WithUnknownId_ThrowsNotFoundException()
    {
        // Arrange
        await using var dbContext = InMemoryMediaDbContext.Create();
        var (resolver, _) = CreateStorage();
        var eventPublisher = CreateEventPublisher();
        var sut = new MediaAttachmentService(dbContext, resolver, CreateUploadOptions("text/plain"), eventPublisher);

        // Act
        Func<Task> act = () => sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_WithUnknownId_ThrowsNotFoundException()
    {
        // Arrange
        await using var dbContext = InMemoryMediaDbContext.Create();
        var (resolver, _) = CreateStorage();
        var eventPublisher = CreateEventPublisher();
        var sut = new MediaAttachmentService(dbContext, resolver, CreateUploadOptions("text/plain"), eventPublisher);

        // Act
        Func<Task> act = () => sut.DeleteAsync(Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_WhenLastReferenceToMediaFile_DeletesFromStorageAndRemovesMediaFile()
    {
        // Arrange
        await using var dbContext = InMemoryMediaDbContext.Create();
        var (resolver, provider) = CreateStorage();
        var eventPublisher = CreateEventPublisher();
        var sut = new MediaAttachmentService(dbContext, resolver, CreateUploadOptions("text/plain"), eventPublisher);
        var attachmentId = await sut.UploadAsync(CreateRequest());
        var mediaFileId = (await dbContext.MediaAttachments.FindAsync(attachmentId))!.MediaFileId;

        // Act
        await sut.DeleteAsync(attachmentId);

        // Assert
        (await dbContext.MediaAttachments.FindAsync(attachmentId)).Should().BeNull();
        (await dbContext.MediaFiles.FindAsync(mediaFileId)).Should().BeNull();
        await provider.Received(1).DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_WithRemainingReferences_KeepsMediaFileAndDoesNotCallStorageDelete()
    {
        // Arrange
        await using var dbContext = InMemoryMediaDbContext.Create();
        var (resolver, provider) = CreateStorage();
        var eventPublisher = CreateEventPublisher();
        var sut = new MediaAttachmentService(dbContext, resolver, CreateUploadOptions("text/plain"), eventPublisher);
        var firstAttachmentId = await sut.UploadAsync(CreateRequest(fileName: "first.txt"));
        var secondAttachmentId = await sut.UploadAsync(CreateRequest(fileName: "second.txt"));
        var mediaFileId = (await dbContext.MediaAttachments.FindAsync(firstAttachmentId))!.MediaFileId;

        // Act
        await sut.DeleteAsync(firstAttachmentId);

        // Assert
        (await dbContext.MediaAttachments.FindAsync(firstAttachmentId)).Should().BeNull();
        (await dbContext.MediaAttachments.FindAsync(secondAttachmentId)).Should().NotBeNull();
        (await dbContext.MediaFiles.FindAsync(mediaFileId)).Should().NotBeNull();
        await provider.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
