//namespace AutoWise.UserVehicles.API.AppService.Services;

//public class UserVehicleService(IUserVehicleRepository userVehicleRepository,
//    VehicleSpecificationsProtoService.VehicleSpecificationsProtoServiceClient vehicleSpecProto,
//    IAuditableDbContext dbContext)
//    : IUserVehicleService
//{
//    private readonly IUserVehicleRepository _userVehicleRepository = userVehicleRepository;
//    private readonly VehicleSpecificationsProtoService.VehicleSpecificationsProtoServiceClient
//        _vehicleSpecProtoService = vehicleSpecProto;
//    private readonly IAuditableDbContext _dbContext = dbContext;

//    public async Task<Guid> AddUserVehicleAsync(AddVehicleRequest request, Guid sessionUserId, CancellationToken cancellationToken)
//    {
//        var getVehicleSpecificationsRequest = new GetVehicleSpecificationsRequest { Vin = request.Vin };
//        var vehicleSpecifications = await _vehicleSpecProtoService.GetVehicleSpecificationsAsync(getVehicleSpecificationsRequest);

//        var vehicle = new UserVehicle
//        {
//            Id = Guid.NewGuid(),
//            UserId = sessionUserId,
//            Vin = request.Vin,
//            LicensePlateNumber = request.LicensePlateNumber,
//            Make = vehicleSpecifications.Specifications?.FirstOrDefault(s => s.Label == "Make")?.Value,
//            Model = vehicleSpecifications.Specifications?.FirstOrDefault(s => s.Label == "Model")?.Value,
//        };

//        if (int.TryParse(vehicleSpecifications.Specifications?.FirstOrDefault(s => s.Label == "Model Year")?.Value, out int year))
//        {
//            vehicle.Year = year;
//        }

//        await _userVehicleRepository.CreateAsync(vehicle, cancellationToken);
//        await _dbContext.SaveChangesAsync(cancellationToken);

//        return vehicle.Id;
//    }
//}
