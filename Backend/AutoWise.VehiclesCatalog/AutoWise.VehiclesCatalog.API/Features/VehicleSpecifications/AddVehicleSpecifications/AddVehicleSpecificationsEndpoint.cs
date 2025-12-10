namespace AutoWise.VehiclesCatalog.API.Features.VehicleSpecifications.AddVehicleSpecifications;

public record AddVehicleSpecificationsRequest(string Vin, IEnumerable<VehicleSpecification> Specifications);
public record AddVehicleSpecificationsResponse(Guid Id, string Vin);

public class AddVehicleSpecificationsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("vehicles-catalog/specifications", async (AddVehicleSpecificationsRequest request, ISender sender) =>
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
