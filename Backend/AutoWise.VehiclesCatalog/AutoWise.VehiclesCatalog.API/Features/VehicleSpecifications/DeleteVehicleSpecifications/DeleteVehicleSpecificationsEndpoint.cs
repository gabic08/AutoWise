using AutoWise.VehiclesCatalog.API.Features.VehicleSpecifications.GetVehicleSpecifications;

namespace AutoWise.VehiclesCatalog.API.Features.VehicleSpecifications.DeleteVehicleSpecifications;

public record DeleteVehicleSpecificationsResponse(bool Success);

public class DeleteVehicleSpecificationsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("vehicles-catalog/specifications/{vin}", async (string vin, ISender sender) =>
        {
            var result = await sender.Send(new DeleteVehicleSpecificationsCommand(vin));

            var response = new DeleteVehicleSpecificationsResponse(result.Success);

            return Results.Ok(response);
        })
        .WithName("DeleteVehicleSpecifications")
        .Produces<GetVehicleSpecificationsResponse>(StatusCodes.Status200OK)
        .WithSummary("Delete vehicle specifications")
        .WithDescription("Delete vehicle specifications by it's VIN");
    }
}