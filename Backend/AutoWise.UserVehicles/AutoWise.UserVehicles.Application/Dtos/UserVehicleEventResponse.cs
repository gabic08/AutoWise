namespace AutoWise.UserVehicles.Application.Dtos;

public record UserVehicleEventResponse(Guid Id, string Name, string Description, DateTime EventDate);
