using AutoWise.CommonUtilities.Exceptions;
using AutoWise.CommonUtilities.Messaging.Contracts.Media;
using AutoWise.UserVehicles.Domain.Models;
using AutoWise.UserVehicles.Infrastructure.Consumers;
using AutoWise.UserVehicles.Tests.TestDoubles;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace AutoWise.UserVehicles.Tests.Infrastructure.Consumers;

public class MediaAttachmentUploadedConsumerTests
{
    private const string ValidVin = "1HGCM82633A004352";

    private static ConsumeContext<MediaAttachmentUploaded> CreateContext(MediaAttachmentUploaded message)
    {
        var context = Substitute.For<ConsumeContext<MediaAttachmentUploaded>>();
        context.Message.Returns(message);
        context.CancellationToken.Returns(CancellationToken.None);
        return context;
    }

    [Fact]
    public async Task Consume_WithNonUserVehicleParentType_DoesNothing()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var vehicle = UserVehicle.Create(Guid.NewGuid(), "ABC-123", "Toyota", "Corolla", ValidVin, 2020);
        dbContext.UserVehicles.Add(vehicle);
        await dbContext.SaveChangesAsync();

        var message = new MediaAttachmentUploaded(Guid.NewGuid(), "SomeOtherParent", vehicle.Id, "file.pdf", "application/pdf", 1024);
        var sut = new MediaAttachmentUploadedConsumer(dbContext);

        // Act
        await sut.Consume(CreateContext(message));

        // Assert
        (await dbContext.UserVehicleAttachments.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Consume_WithUnknownVehicle_ThrowsNotFoundException()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var message = new MediaAttachmentUploaded(Guid.NewGuid(), "UserVehicle", Guid.NewGuid(), "file.pdf", "application/pdf", 1024);
        var sut = new MediaAttachmentUploadedConsumer(dbContext);

        // Act
        Func<Task> act = () => sut.Consume(CreateContext(message));

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Consume_WithValidMessage_AddsAttachmentToVehicle()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var vehicle = UserVehicle.Create(Guid.NewGuid(), "ABC-123", "Toyota", "Corolla", ValidVin, 2020);
        dbContext.UserVehicles.Add(vehicle);
        await dbContext.SaveChangesAsync();

        var mediaAttachmentId = Guid.NewGuid();
        var message = new MediaAttachmentUploaded(mediaAttachmentId, "UserVehicle", vehicle.Id, "invoice.pdf", "application/pdf", 2048);
        var sut = new MediaAttachmentUploadedConsumer(dbContext);

        // Act
        await sut.Consume(CreateContext(message));

        // Assert
        var attachment = await dbContext.UserVehicleAttachments.FirstOrDefaultAsync(a => a.UserVehicleId == vehicle.Id);
        attachment.Should().NotBeNull();
        attachment!.MediaAttachmentId.Should().Be(mediaAttachmentId);
        attachment.OriginalFileName.Should().Be("invoice.pdf");
        attachment.ContentType.Should().Be("application/pdf");
        attachment.SizeInBytes.Should().Be(2048);
    }
}
