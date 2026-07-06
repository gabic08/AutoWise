namespace AutoWise.UserVehicles.API.Persistence.Data;

public class UserVehiclesDbContext(
    DbContextOptions<UserVehiclesDbContext> options,
    IHttpContextAccessor httpContextAccessor)
    : AuditableDbContext(options, httpContextAccessor), IAuditableDbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureDatabaseWithSchema("UserVehicles");
    }
}
