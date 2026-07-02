namespace AutoWise.UserVehicles.API.Domain;

public class UserVehicle : ModifiedCreatedAuditBaseEntity
{
    public string LicensePlateNumber { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public Guid UserId { get; set; }
    public string Vin { get; set; }
    public int? Year { get; set; }
}
