using AutoWise.CommonUtilities.Persistence.PostgreSQL.Context;
using AutoWise.Users.Application;
using AutoWise.Users.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoWise.Users.Infrastructure.Data;

public class UsersDbContext(DbContextOptions<UsersDbContext> options) :
    DbContext(options), IUsersDbContext
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureDatabaseWithSchema(InfrastructureDataConstants.UsersSchema);
    }
}
