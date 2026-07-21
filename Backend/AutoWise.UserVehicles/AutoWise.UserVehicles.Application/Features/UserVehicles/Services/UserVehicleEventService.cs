namespace AutoWise.UserVehicles.Application.Features.UserVehicles.Services;

public class UserVehicleEventService(IUserVehiclesDbContext dbContext) : IUserVehicleEventService
{
    private readonly IUserVehiclesDbContext _dbContext = dbContext;

    public async Task<Guid> AddEventAsync(Guid vehicleId, CreateUserVehicleEventRequest request, CancellationToken ct = default)
    {
        var vehicle = await _dbContext.UserVehicles.FirstOrDefaultAsync(v => v.Id == vehicleId, ct)
            ?? throw new NotFoundException($"User vehicle with id '{vehicleId}' was not found.");

        var vehicleEvent = vehicle.AddEvent(request.Name, request.Description, request.EventDate);

        await _dbContext.SaveChangesAsync(ct);

        return vehicleEvent.Id;
    }

    public async Task<UserVehicleEventResponse> GetEventAsync(Guid vehicleId, Guid eventId, CancellationToken ct = default)
    {
        var vehicle = await _dbContext.UserVehicles
            .AsNoTracking()
            .Include(uv => uv.UserVehicleEvents.Where(uve => uve.Id == eventId))
            .FirstOrDefaultAsync(v => v.Id == vehicleId, ct)
            ?? throw new NotFoundException($"User vehicle with id '{vehicleId}' was not found.");

        var vehicleEvent = vehicle.UserVehicleEvents.FirstOrDefault(e => e.Id == eventId)
            ?? throw new NotFoundException($"Vehicle event with id '{eventId}' was not found.");

        return new UserVehicleEventResponse(
            vehicleEvent.Id,
            vehicleEvent.Name,
            vehicleEvent.Description,
            vehicleEvent.EventDate);
    }

    public async Task UpdateEventAsync(Guid vehicleId, Guid eventId, UpdateUserVehicleEventRequest request, CancellationToken ct = default)
    {
        var vehicle = await _dbContext.UserVehicles
            .Include(uv => uv.UserVehicleEvents.Where(uve => uve.Id == eventId))
            .FirstOrDefaultAsync(v => v.Id == vehicleId, ct)
            ?? throw new NotFoundException($"User vehicle with id '{vehicleId}' was not found.");

        if (!vehicle.UserVehicleEvents.Any(e => e.Id == eventId))
        {
            throw new NotFoundException($"Vehicle event with id '{eventId}' was not found.");
        }

        vehicle.UpdateEvent(eventId, request.Name, request.Description, request.EventDate);

        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task RemoveEventAsync(Guid vehicleId, Guid eventId, CancellationToken ct = default)
    {
        var vehicle = await _dbContext.UserVehicles
            .Include(uv => uv.UserVehicleEvents.Where(uve => uve.Id == eventId))
            .FirstOrDefaultAsync(v => v.Id == vehicleId, ct)
            ?? throw new NotFoundException($"User vehicle with id '{vehicleId}' was not found.");

        if (!vehicle.UserVehicleEvents.Any(e => e.Id == eventId))
        {
            throw new NotFoundException($"Vehicle event with id '{eventId}' was not found.");
        }

        vehicle.RemoveEvent(eventId);

        await _dbContext.SaveChangesAsync(ct);
    }
}
