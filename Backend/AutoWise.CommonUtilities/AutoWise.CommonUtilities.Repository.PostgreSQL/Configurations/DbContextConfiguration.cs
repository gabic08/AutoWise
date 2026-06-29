using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AutoWise.CommonUtilities.Repository.PostgreSQL.Configurations;

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

    internal static void ConfigureEntityConversionToUtc(this ModelBuilder modelBuilder)
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
