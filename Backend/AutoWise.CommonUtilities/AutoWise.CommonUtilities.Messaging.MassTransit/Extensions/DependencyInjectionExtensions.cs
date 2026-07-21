using AutoWise.CommonUtilities.Messaging.Abstractions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AutoWise.CommonUtilities.Messaging.MassTransit.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddMassTransitMessaging<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        OutboxDatabaseProvider outboxDatabaseProvider,
        Action<IBusRegistrationConfigurator> configureConsumers = null) where TDbContext : DbContext
    {
        var connectionString = configuration.GetConnectionString("RabbitMQ")
            ?? throw new InvalidOperationException("Connection string 'RabbitMQ' not found.");

        services.AddMassTransit(x =>
        {
            x.AddEntityFrameworkOutbox<TDbContext>(o =>
            {
                switch (outboxDatabaseProvider)
                {
                    case OutboxDatabaseProvider.Postgres:
                        o.UsePostgres();
                        break;
                    case OutboxDatabaseProvider.SqlServer:
                        o.UseSqlServer();
                        break;
                    case OutboxDatabaseProvider.MySql:
                        o.UseMySql();
                        break;
                    case OutboxDatabaseProvider.Sqlite:
                        o.UseSqlite();
                        break;
                    case OutboxDatabaseProvider.Oracle:
                        o.UseOracle();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(outboxDatabaseProvider));
                }

                o.UseBusOutbox();
            });

            configureConsumers?.Invoke(x);

            x.AddConfigureEndpointsCallback((context, name, cfg) =>
            {
                cfg.UseEntityFrameworkOutbox<TDbContext>(context);
            });

            x.UsingRabbitMq((context, config) =>
            {
                config.Host(new Uri(connectionString));
                config.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

        return services;
    }
}
