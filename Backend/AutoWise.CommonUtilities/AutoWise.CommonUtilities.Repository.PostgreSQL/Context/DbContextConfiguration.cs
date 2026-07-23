using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AutoWise.CommonUtilities.Persistence.PostgreSQL.Context;

public static class DbContextConfiguration
{
    public static void ConfigureDatabaseWithSchema(this ModelBuilder modelBuilder, string schemaName, bool configureEntityConversionToUtc = true)
    {
        modelBuilder.HasDefaultSchema(schemaName);

        if (configureEntityConversionToUtc)
        {
            modelBuilder.ConfigureEntityConversionToUtc();
        }

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetCallingAssembly());
    }

    public static async Task ApplyMigrationsAsync<TDbContext>(this WebApplication app) where TDbContext : DbContext
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        await context.Database.MigrateAsync();
    }

    private static void ConfigureEntityConversionToUtc(this ModelBuilder modelBuilder)
    {
        // Apply UTC timestamp convention globally
        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(e => e.GetProperties())
            .Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?)))
        {
            property.SetColumnType("timestamp with time zone");
        }
    }
}
