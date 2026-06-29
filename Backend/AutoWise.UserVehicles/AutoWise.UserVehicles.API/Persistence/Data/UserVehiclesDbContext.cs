using AutoWise.CommonUtilities.Repository.PostgreSQL.Configurations;
using Microsoft.EntityFrameworkCore;

namespace AutoWise.UserVehicles.API.Persistence.Data;

public class UserVehiclesDbContext(DbContextOptions<UserVehiclesDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureDatabaseWithSchema("UserVehicles");
    }
}
