namespace AutoWise.VehiclesCatalog.API.Features.VehicleSpecifications.GetVehicleSpecifications;

public record GetVehicleSpecificationsResponse(IEnumerable<VehicleSpecification> Specifications);

public class GetVehicleSpecificationsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("vehicles-catalog/specifications/{vin}", async (string vin, ISender sender) =>
        {
            var result = await sender.Send(new GetVehicleSpecificationsCommand(vin));

            var response = new GetVehicleSpecificationsResponse(result.Specifications);

            return Results.Ok(response);
        })
        .WithName("GetVehicleSpecifications")
        .Produces<GetVehicleSpecificationsResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Get vehicle specifications")
        .WithDescription("Get vehicle specifications by it's VIN");
    }
}