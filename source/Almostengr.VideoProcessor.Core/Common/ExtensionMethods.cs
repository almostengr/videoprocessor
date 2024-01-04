namespace Almostengr.VideoProcessor.Core.Common;

public static class ExtensionMethods
{
    public static string ReplaceIgnoringCase(this string haystack, string oldValue, string newValue)
    {
        return haystack.Replace(oldValue, newValue, StringComparison.OrdinalIgnoreCase);
    }

    public static bool ContainsIgnoringCase(this string haystack, string needle)
    {
        if (string.IsNullOrEmpty(haystack))
        {
            return false;
        }
        
        return haystack.Contains(needle, StringComparison.OrdinalIgnoreCase);
    }

    public static bool EndsWithIgnoringCase(this string haystack, string needle)
    {
        if (string.IsNullOrEmpty(haystack))
        {
            return false;
        }
        
        return haystack.EndsWith(needle, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsNotNullOrWhiteSpace(this string value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    public static bool DoesNotContainIgnoringCase(this string haystack, string needle)
    {
        return !haystack.Contains(needle, StringComparison.OrdinalIgnoreCase);
    }
}