using System.Text.Json.Serialization;

namespace AutoWise.VehiclesCatalog.API.VehicleConfigurations;

public class VehicleSpecificationRequest
{
    [JsonPropertyName("label")]
    public string Label { get; set; }

    [JsonPropertyName("value")]
    public JsonElement Value { get; set; }
}
