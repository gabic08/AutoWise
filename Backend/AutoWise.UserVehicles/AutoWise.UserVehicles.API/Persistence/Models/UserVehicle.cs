using AutoWise.CommonUtilities.Models.BaseEntities;

namespace AutoWise.UserVehicles.API.Persistence.Models;

public class UserVehicle : ModifiedCreatedAuditBaseEntity
{
    public Guid UserId { get; set; }
    public string Vin { get; set; }
    public int? Year { get; set; }
    public string Model { get; set; }
    public string Make { get; set; }
    public string ProductType { get; set; }
    public string Body { get; set; }
    public string LicensePlateNumber { get; set; }
}
