namespace AutoWise.UserVehicles.Infrastructure.Data;

public class UserVehiclesDbContext(DbContextOptions<UserVehiclesDbContext> options)
    : DbContext(options), IUserVehiclesDbContext
{
    public DbSet<UserVehicle> UserVehicles => Set<UserVehicle>();
    public DbSet<UserVehicleEvent> UserVehicleEvents => Set<UserVehicleEvent>();
    public DbSet<UserVehicleAttachment> UserVehicleAttachments => Set<UserVehicleAttachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureDatabaseWithSchema(InfrastructureDataConstants.UserVehiclesSchema);
        modelBuilder.AddMassTransitInboxOutboxEntities();
    }
}
