using AutoWise.VehiclesCatalog.API.Features.VehicleSpecifications.AddVehicleSpecifications;

namespace AutoWise.VehiclesCatalog.API.Features.VehicleIdentificationNumber.AddVin;

public record AddVinRequest(string Vin);
public record AddVinResponse(IEnumerable<VehicleSpecification> Specifications);

public class AddVinEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("vehicles-catalog/vin", async (AddVinRequest request, ISender sender) =>
        {
            var result = await sender.Send(new AddVinCommand(request.Vin));

            var response = new AddVinResponse(result.Specifications);

            return Results.Created($"/vehicles-catalog/vin/{request.Vin}", response);
        })
        .WithName("AddVin")
        .Produces<AddVehicleSpecificationsResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Import vehicle specifications")
        .WithDescription("Import vehicle specifications by it's VIN");
    }
}