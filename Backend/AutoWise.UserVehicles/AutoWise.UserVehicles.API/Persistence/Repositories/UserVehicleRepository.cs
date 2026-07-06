using AutoWise.UserVehicles.Domain.Models;

namespace AutoWise.UserVehicles.API.Persistence.Repositories;

public class UserVehicleRepository(UserVehiclesDbContext dbContext) : GenericRepository<UserVehicle>(dbContext), IUserVehicleRepository
{
}
