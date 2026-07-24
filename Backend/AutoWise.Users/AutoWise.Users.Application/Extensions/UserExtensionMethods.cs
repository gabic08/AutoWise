namespace AutoWise.Users.Application.Extensions;

public static class UserExtensionMethods
{
    public static bool ProfileNeedsUpdate(this User user, string email, string displayName)
    {
        return user.DisplayName != displayName || user.Email != email;
    }
}
