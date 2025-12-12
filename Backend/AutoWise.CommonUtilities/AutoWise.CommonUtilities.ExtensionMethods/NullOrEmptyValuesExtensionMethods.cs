namespace AutoWise.CommonUtilities.ExtensionMethods;

public static class NullOrEmptyValuesExtensionMethods
{
    public static bool NotNullOrEmpty<T>(this IEnumerable<T> source)
    {
        return source is not null && source.Any();
    }

    public static bool NullOrEmpty<T>(this IEnumerable<T> source)
    {
        return source is null || !source.Any();
    }

    public static bool NotNullOrEmpty<T>(this ICollection<T> source)
    {
        return source is not null && source.Count > 0;
    }

    public static bool NullOrEmpty<T>(this ICollection<T> source)
    {
        return source is null || source.Count == 0;
    }

    public static bool NotNullOrEmpty(this Guid? source)
    {
        return source is not null && source != Guid.Empty;
    }

    public static bool NullOrEmpty(this Guid? source)
    {
        return source is null || source == Guid.Empty;
    }

    public static bool NotNullOrEmpty(this Guid source)
    {
        return source != Guid.Empty;
    }

    public static bool NullOrEmpty(this Guid source)
    {
        return source == Guid.Empty;
    }

    public static bool NotNullOrEmpty(this string source)
    {
        return !string.IsNullOrWhiteSpace(source);
    }

    public static bool NullOrEmpty(this string source)
    {
        return string.IsNullOrWhiteSpace(source);
    }
}
