namespace AutoWise.VehiclesCatalog.API.Models;

public class Vehicle
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    [BsonElement("_id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [BsonElement("_vin")]
    public string Vin { get; set; }

    [BsonElement("_specifications")]
    public List<VehicleSpecification> Specifications { get; set; } = [];
}
