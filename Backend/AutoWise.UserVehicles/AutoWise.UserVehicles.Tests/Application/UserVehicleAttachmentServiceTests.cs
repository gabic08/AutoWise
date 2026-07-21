using AutoWise.CommonUtilities.Exceptions;
using AutoWise.CommonUtilities.Messaging.Abstractions;
using AutoWise.CommonUtilities.Messaging.Contracts.Media;
using AutoWise.UserVehicles.Application.Features.UserVehicles.Services;
using AutoWise.UserVehicles.Domain.Models;
using AutoWise.UserVehicles.Tests.TestDoubles;
using FluentAssertions;
using NSubstitute;

namespace AutoWise.UserVehicles.Tests.Application;

public class UserVehicleAttachmentServiceTests
{
    private const string ValidVin = "1HGCM82633A004352";

    private static IEventPublisher CreateEventPublisher()
    {
        return Substitute.For<IEventPublisher>();
    }

    private static async Task<(UserVehicle Vehicle, UserVehicleAttachment Attachment)> SeedVehicleWithAttachmentAsync(
        InMemoryUserVehiclesDbContext dbContext)
    {
        var vehicle = UserVehicle.Create(Guid.NewGuid(), "ABC-123", "Toyota", "Corolla", ValidVin, 2020);
        var attachment = vehicle.AddAttachment(Guid.NewGuid(), "invoice.pdf", "application/pdf", 2048);
        dbContext.UserVehicles.Add(vehicle);
        await dbContext.SaveChangesAsync();

        return (vehicle, attachment);
    }

    [Fact]
    public async Task RemoveAttachmentAsync_WithExistingAttachment_RemovesAttachmentAndPublishesEvent()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var (vehicle, attachment) = await SeedVehicleWithAttachmentAsync(dbContext);
        var eventPublisher = CreateEventPublisher();
        var sut = new UserVehicleAttachmentService(dbContext, eventPublisher);

        // Act
        await sut.RemoveAttachmentAsync(vehicle.Id, attachment.Id);

        // Assert
        var deletedAttachment = await dbContext.UserVehicleAttachments.FindAsync(attachment.Id);
        deletedAttachment.Should().BeNull();

        await eventPublisher.Received(1).PublishAsync(
            Arg.Is<MediaAttachmentRemoved>(e => e.MediaAttachmentId == attachment.MediaAttachmentId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveAttachmentAsync_WithUnknownVehicle_ThrowsNotFoundException()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var sut = new UserVehicleAttachmentService(dbContext, CreateEventPublisher());

        // Act
        Func<Task> act = () => sut.RemoveAttachmentAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task RemoveAttachmentAsync_WithUnknownAttachment_ThrowsNotFoundException()
    {
        // Arrange
        await using var dbContext = InMemoryUserVehiclesDbContext.Create();
        var (vehicle, _) = await SeedVehicleWithAttachmentAsync(dbContext);
        var sut = new UserVehicleAttachmentService(dbContext, CreateEventPublisher());

        // Act
        Func<Task> act = () => sut.RemoveAttachmentAsync(vehicle.Id, Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
