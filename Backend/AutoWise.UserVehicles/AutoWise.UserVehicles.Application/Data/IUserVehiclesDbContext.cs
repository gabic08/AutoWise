using AutoWise.UserVehicles.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoWise.UserVehicles.Application.Data;

public interface IUserVehiclesDbContext
{
    DbSet<UserVehicle> UserVehicles { get; }
    DbSet<UserVehicleEvent> UserVehicleEvents { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
