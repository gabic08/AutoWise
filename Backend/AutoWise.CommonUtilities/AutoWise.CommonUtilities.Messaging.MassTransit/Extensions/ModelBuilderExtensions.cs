using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace AutoWise.CommonUtilities.Messaging.MassTransit.Extensions;

public static class ModelBuilderExtensions
{
    public static void AddMassTransitInboxOutboxEntities(this ModelBuilder modelBuilder)
    {
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
