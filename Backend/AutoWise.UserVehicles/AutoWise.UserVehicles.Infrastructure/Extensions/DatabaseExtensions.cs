using AutoWise.UserVehicles.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoWise.UserVehicles.Infrastructure.Extensions;

public static class DatabaseExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UserVehiclesDbContext>();
        await context.Database.MigrateAsync();
    }
}
