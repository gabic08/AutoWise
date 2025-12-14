using System.Security.Cryptography;
using System.Text;

namespace AutoWise.VehiclesCatalog.API.VehicleConfigurations;

public class GetVehicleSpecificationsConfig(IConfiguration configuration)
{
    private readonly VinDecoderSpecifications _vinDecoderSpecifications = configuration.GetSection("VinDecoderSpecifications").Get<VinDecoderSpecifications>();

    public string GetUrl(string vin)
    {
        var controlSum = CalculateControlSumParameter(vin, _vinDecoderSpecifications.Id, _vinDecoderSpecifications.ApiKey, _vinDecoderSpecifications.SecretKey);
        string url = $"{_vinDecoderSpecifications.ApiPrefix}/{_vinDecoderSpecifications.ApiKey}/{controlSum}/decode/{vin}.json";
        return url;
    }

    private static string CalculateControlSumParameter(string vin, string id, string apiKey, string secretKey)
    {
        string input = $"{vin}|{id}|{apiKey}|{secretKey}";
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        byte[] hash = SHA1.HashData(bytes);
        string hex = Convert.ToHexStringLower(hash);
        return hex[..10];
    }
}
