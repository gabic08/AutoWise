namespace AutoWise.UserVehicles.Application.Data;

public interface IUserVehiclesDbContext : IBaseDbContext
{
    DbSet<UserVehicle> UserVehicles { get; }
    DbSet<UserVehicleEvent> UserVehicleEvents { get; }
    DbSet<UserVehicleAttachment> UserVehicleAttachments { get; }
}
