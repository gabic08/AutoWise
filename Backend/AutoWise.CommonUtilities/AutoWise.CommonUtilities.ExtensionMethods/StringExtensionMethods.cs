namespace AutoWise.CommonUtilities.ExtensionMethods;

public static class StringExtensionMethods
{
    public static bool EqualsCaseInsensitive(this string a, string b)
    {
        return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }

    public static bool NotEqualsCaseInsensitive(this string a, string b)
    {
        return !string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }
}
