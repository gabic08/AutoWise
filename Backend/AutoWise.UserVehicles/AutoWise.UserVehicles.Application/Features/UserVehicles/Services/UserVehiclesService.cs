namespace AutoWise.UserVehicles.Application.Features.UserVehicles.Services;

public class UserVehiclesService(IUserVehiclesDbContext dbContext, IVehicleSpecificationsService vehicleSpecificationsService)
    : IUserVehiclesService
{
    private readonly IUserVehiclesDbContext _dbContext = dbContext;
    private readonly IVehicleSpecificationsService _vehicleSpecificationsService = vehicleSpecificationsService;


    public async Task<Guid> CreateAsync(CreateUserVehicleRequest request, Guid sessionUserId, CancellationToken ct = default)
    {
        var vehicleSpecifications = await _vehicleSpecificationsService.GetSpecificationsAsync(request.Vin, ct);

        var licensePlateNumber = request.LicensePlateNumber;
        var vin = request.Vin;
        var make = vehicleSpecifications.FirstOrDefault(s => s.Label == "Make")?.Value;
        var model = vehicleSpecifications.FirstOrDefault(s => s.Label == "Model")?.Value;
        _ = int.TryParse(
            vehicleSpecifications.FirstOrDefault(s => s.Label == "Model Year")?.Value,
            out int year);


        var newVehicle = UserVehicle.Create(sessionUserId, licensePlateNumber, make, model, vin, year);


        await _dbContext.UserVehicles.AddAsync(newVehicle, ct);
        await _dbContext.SaveChangesAsync(ct);

        return newVehicle.Id;
    }

    public async Task<UserVehicleResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var vehicle = await _dbContext.UserVehicles
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id, ct)
            ?? throw new NotFoundException($"User vehicle with id '{id}' was not found.");

        return new UserVehicleResponse(vehicle.Id, vehicle.LicensePlateNumber);
    }

    public async Task UpdateAsync(Guid id, UpdateUserVehicleRequest request, CancellationToken ct = default)
    {
        var vehicle = await _dbContext.UserVehicles.FirstOrDefaultAsync(v => v.Id == id, ct)
            ?? throw new NotFoundException($"User vehicle with id '{id}' was not found.");

        vehicle.ChangeLicensePlateNumber(request.LicensePlateNumber);

        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var vehicle = await _dbContext.UserVehicles.FirstOrDefaultAsync(v => v.Id == id, ct)
            ?? throw new NotFoundException($"User vehicle with id '{id}' was not found.");

        _dbContext.UserVehicles.Remove(vehicle);
        await _dbContext.SaveChangesAsync(ct);
    }
}
