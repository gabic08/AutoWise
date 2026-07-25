namespace AutoWise.YarpApiGateway.Auth;

public class KeycloakAuthOptions
{
    public const string SectionName = "Auth:Keycloak";

    public string Authority { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}
