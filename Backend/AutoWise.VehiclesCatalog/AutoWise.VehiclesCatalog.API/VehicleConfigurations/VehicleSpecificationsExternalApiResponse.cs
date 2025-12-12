using System.Text.Json.Serialization;

namespace AutoWise.VehiclesCatalog.API.VehicleConfigurations;

public class VehicleSpecificationsExternalApiResponse
{
    [JsonPropertyName("decode")]
    public List<VehicleSpecificationRequest> Decode { get; set; }
}
