using AutoWise.CommonUtilities.Messaging.Contracts.Media;
using AutoWise.Media.Application.Features.MediaAttachments;
using AutoWise.Media.Infrastructure.Consumers;
using MassTransit;
using NSubstitute;

namespace AutoWise.Media.Tests.Infrastructure.Consumers;

public class MediaAttachmentRemovedConsumerTests
{
    private static ConsumeContext<MediaAttachmentRemoved> CreateContext(MediaAttachmentRemoved message)
    {
        var context = Substitute.For<ConsumeContext<MediaAttachmentRemoved>>();
        context.Message.Returns(message);
        context.CancellationToken.Returns(CancellationToken.None);
        return context;
    }

    [Fact]
    public async Task Consume_CallsDeleteAsyncWithMessageMediaAttachmentId()
    {
        // Arrange
        var mediaAttachmentService = Substitute.For<IMediaAttachmentService>();
        var mediaAttachmentId = Guid.NewGuid();
        var message = new MediaAttachmentRemoved(mediaAttachmentId);
        var sut = new MediaAttachmentRemovedConsumer(mediaAttachmentService);

        // Act
        await sut.Consume(CreateContext(message));

        // Assert
        await mediaAttachmentService.Received(1).DeleteAsync(mediaAttachmentId, Arg.Any<CancellationToken>());
    }
}
