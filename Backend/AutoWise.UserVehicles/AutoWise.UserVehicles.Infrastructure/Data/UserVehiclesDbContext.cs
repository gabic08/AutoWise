using AutoWise.CommonUtilities.Persistence.PostgreSQL.Context;
using AutoWise.UserVehicles.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoWise.UserVehicles.Infrastructure.Data;

public class UserVehiclesDbContext(DbContextOptions<UserVehiclesDbContext> options) : DbContext(options)
{
    public DbSet<UserVehicle> UserVehicles => Set<UserVehicle>();
    public DbSet<UserVehicleEvent> UserVehicleEvents => Set<UserVehicleEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureDatabaseWithSchema(InfrastructureDataConstants.UserVehiclesSchema);
    }
}
