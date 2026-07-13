namespace AutoWise.VehiclesCatalog.API.Features.VehicleSpecifications.ImportVehicleSpecifications;

public record ImportVehicleSpecificationsRequest(string Vin);
public record ImportVehicleSpecificationsResponse(IEnumerable<VehicleSpecification> Specifications);

public class ImportVehicleSpecificationsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/vehicles-catalog/specifications/import", async (ImportVehicleSpecificationsRequest request, ISender sender) =>
        {
            var result = await sender.Send(new ImportVehicleSpecificationsCommand(request.Vin));

            var response = new ImportVehicleSpecificationsResponse(result.Specifications);

            return Results.Created($"/api/vehicles-catalog/specifications/{request.Vin}", response);
        })
        .WithName("ImportVehicleSpecifications")
        .Produces<ImportVehicleSpecificationsResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Import vehicle specifications")
        .WithDescription("Import vehicle specifications by it's VIN");
    }
}
