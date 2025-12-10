using AutoWise.VehiclesCatalog.API.Features.VehicleSpecifications.AddVehicleSpecifications;

namespace AutoWise.VehiclesCatalog.API.Features.VehicleIdentificationNumber.AddVin;

public record AddVinRequest(string Vin, List<VehicleSpecification> Specifications);
public record AddVinResponse(IEnumerable<VehicleSpecification> Specifications);

public class AddVinEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("vehicles-catalog/vin", async (AddVinRequest request, ISender sender) =>
        {
            var result = await sender.Send(new AddVehicleSpecificationsCommand(request.Vin, request.Specifications));

            var response = new AddVehicleSpecificationsResponse(result.Id, result.Vin);

            return Results.Created($"/vehicle-specifications/{request.Vin}", response);
        })
        .WithName("AddVehicleSpecifications")
        .Produces<AddVehicleSpecificationsResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Add Vehicle Specifications")
        .WithDescription("This endpoint allows you to add a JSON with vehicle specifications if you don't have to call a paid API fot that VIN");
    }
}