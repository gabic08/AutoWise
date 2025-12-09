namespace AutoWise.VehiclesCatalog.API.Models;

public class VehicleSpecification
{
    [BsonElement("_label")]
    public string Label { get; set; }

    [BsonElement("_value")]
    public string Value { get; set; }
}
