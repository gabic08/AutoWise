using AutoWise.UserVehicles.Application.Data;
using AutoWise.UserVehicles.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoWise.UserVehicles.Tests.TestDoubles;

public class InMemoryUserVehiclesDbContext(DbContextOptions<InMemoryUserVehiclesDbContext> options)
    : DbContext(options), IUserVehiclesDbContext
{
    public DbSet<UserVehicle> UserVehicles => Set<UserVehicle>();
    public DbSet<UserVehicleEvent> UserVehicleEvents => Set<UserVehicleEvent>();

    public static InMemoryUserVehiclesDbContext Create()
    {
        var options = new DbContextOptionsBuilder<InMemoryUserVehiclesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new InMemoryUserVehiclesDbContext(options);
    }
}
