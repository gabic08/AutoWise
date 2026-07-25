namespace AutoWise.UserVehicles.Application.Services;

public class UserVehiclesService(
    IUserVehiclesDbContext dbContext,
    IVehicleSpecificationsService vehicleSpecificationsService,
    IDistributedCache cache)
    : IUserVehiclesService
{
    private readonly IUserVehiclesDbContext _dbContext = dbContext;
    private readonly IVehicleSpecificationsService _vehicleSpecificationsService = vehicleSpecificationsService;
    private readonly IDistributedCache _cache = cache;


    public async Task<Guid> CreateAsync(CreateUserVehicleRequest request, Guid sessionUserId, CancellationToken ct = default)
    {
        var vehicleSpecifications = await GetVehicleSpecificationsAsync(request.Vin, ct);

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

    private async Task<IEnumerable<VehicleSpecificationDto>> GetVehicleSpecificationsAsync(string vin, CancellationToken ct)
    {
        var cacheKey = $"vehicle-specifications:{vin}";

        var cachedValue = await _cache.GetStringAsync(cacheKey, ct);
        if (!cachedValue.NullOrEmpty())
        {
            return JsonSerializer.Deserialize<IEnumerable<VehicleSpecificationDto>>(cachedValue)!;
        }

        return await _vehicleSpecificationsService.GetSpecificationsAsync(vin, ct);
    }

    public async Task<QueryResponse<UserVehicleResponse>> GetAllForUserAsync(Guid sessionUserId, GetUserVehiclesRequest request, CancellationToken ct = default)
    {
        var query = _dbContext.UserVehicles.AsNoTracking().Where(v => v.UserId == sessionUserId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(v =>
                v.Make.ToLower().Contains(search) ||
                v.Model.ToLower().Contains(search) ||
                v.LicensePlateNumber.ToLower().Contains(search) ||
                v.Vin.ToLower().Contains(search));
        }

        query = request.SortBy?.Trim().ToLowerInvariant() switch
        {
            "make" => request.SortDescending ? query.OrderByDescending(v => v.Make) : query.OrderBy(v => v.Make),
            "model" => request.SortDescending ? query.OrderByDescending(v => v.Model) : query.OrderBy(v => v.Model),
            "year" => request.SortDescending ? query.OrderByDescending(v => v.Year) : query.OrderBy(v => v.Year),
            "licenseplatenumber" => request.SortDescending ? query.OrderByDescending(v => v.LicensePlateNumber) : query.OrderBy(v => v.LicensePlateNumber),
            _ => query.OrderByDescending(v => v.CreatedOn)
        };

        var totalItemsCount = await query.CountAsync(ct);

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize switch
        {
            < 1 => 20,
            > 100 => 100,
            _ => request.PageSize
        };

        var vehicles = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new UserVehicleResponse(v.Id, v.LicensePlateNumber, v.Make, v.Model, v.Vin, v.Year))
            .ToListAsync(ct);

        var pagedResponse = new PagedQueryResponse
        {
            CurrentPage = page,
            PageSize = pageSize,
            TotalItemsCount = totalItemsCount,
            TotalPagesCount = (int)Math.Ceiling(totalItemsCount / (double)pageSize)
        };

        return new QueryResponse<UserVehicleResponse>(vehicles, pagedResponse);
    }

    public async Task<UserVehicleResponse> GetByIdAsync(Guid id, Guid sessionUserId, CancellationToken ct = default)
    {
        var vehicle = await _dbContext.UserVehicles
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id && v.UserId == sessionUserId, ct)
            ?? throw new NotFoundException($"User vehicle with id '{id}' was not found.");

        return new UserVehicleResponse(vehicle.Id, vehicle.LicensePlateNumber, vehicle.Make, vehicle.Model, vehicle.Vin, vehicle.Year);
    }

    public async Task UpdateAsync(Guid id, UpdateUserVehicleRequest request, Guid sessionUserId, CancellationToken ct = default)
    {
        var vehicle = await _dbContext.UserVehicles.FirstOrDefaultAsync(v => v.Id == id && v.UserId == sessionUserId, ct)
            ?? throw new NotFoundException($"User vehicle with id '{id}' was not found.");

        vehicle.ChangeLicensePlateNumber(request.LicensePlateNumber);

        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, Guid sessionUserId, CancellationToken ct = default)
    {
        var vehicle = await _dbContext.UserVehicles.FirstOrDefaultAsync(v => v.Id == id && v.UserId == sessionUserId, ct)
            ?? throw new NotFoundException($"User vehicle with id '{id}' was not found.");

        _dbContext.UserVehicles.Remove(vehicle);
        await _dbContext.SaveChangesAsync(ct);
    }
}
