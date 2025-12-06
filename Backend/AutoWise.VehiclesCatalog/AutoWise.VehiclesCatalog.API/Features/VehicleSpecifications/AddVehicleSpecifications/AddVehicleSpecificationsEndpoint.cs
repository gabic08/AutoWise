using Carter;
using MediatR;

namespace AutoWise.VehiclesCatalog.API.Features.VehicleSpecifications.AddVehicleSpecifications;

public record AddVehicleSpecificationsRequest(string Vin);

public record AddVehicleSpecificationsResponse(string Vin);

public class AddVehicleSpecificationsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/vehicle-specifications", async (AddVehicleSpecificationsRequest request, ISender sender) =>
        {
            var result = await sender.Send(new AddVehicleSpecificationsCommand(request.Vin));

            var response = new AddVehicleSpecificationsResponse(result.Vin);

            return Results.Created($"/vehicle-specifications/{request.Vin}", response);
        
        })
        .WithName("AddVehicleSpecifications")
        .Produces<AddVehicleSpecificationsResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Add Vehicle Specifications")
        .WithDescription("This endpoint allows you to add a json with vehicle specifications wo you don't have to call a paid API");
    }
}
