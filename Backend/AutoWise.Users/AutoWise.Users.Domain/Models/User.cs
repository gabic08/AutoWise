using AutoWise.CommonUtilities.Models.BaseEntities;

namespace AutoWise.Users.Domain.Models;

public class User : ModifiedCreatedAuditBaseEntity
{
    public string DisplayName { get; private set; }
    public string Email { get; private set; }
    public string ExternalId { get; private set; }
    public string Provider { get; private set; }

    public User() { }

    public static User Create(string displayName, string email, string externalId, string provider)
    {
        var user = new User();

        user.SetDisplayName(displayName);
        user.SetEmail(email);
        user.SetExternalId(externalId);
        user.SetProvider(provider);

        return user;
    }

    public void UpdateProfile(string email, string displayName)
    {
        SetEmail(email);
        SetDisplayName(displayName);
    }

    private void SetExternalId(string externalId)
    {
        if (string.IsNullOrWhiteSpace(externalId))
        {
            throw new ArgumentException("External id is required.", nameof(externalId));
        }
        ExternalId = externalId.Trim();
    }

    private void SetProvider(string provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            throw new ArgumentException("Provider is required.", nameof(provider));
        }
        Provider = provider.Trim();
    }

    private void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }
        Email = email.Trim();
    }

    private void SetDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Display name is required.", nameof(displayName));
        }
        DisplayName = displayName.Trim();
    }
}
