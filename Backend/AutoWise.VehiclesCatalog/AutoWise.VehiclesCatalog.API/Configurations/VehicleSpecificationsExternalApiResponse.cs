using System.Text.Json.Serialization;

namespace AutoWise.VehiclesCatalog.API.Configurations;

public class VehicleSpecificationsExternalApiResponse
{
    [JsonPropertyName("decode")]
    public List<VehicleSpecificationRequest> Decode { get; set; }
}
