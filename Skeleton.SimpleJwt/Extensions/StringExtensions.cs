using System.Diagnostics.CodeAnalysis;

namespace Skeleton.SimpleJwt.Extensions
{
    internal static class StringExtensions
    {
        public static bool IsEqualTo(this string? a, string? b, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (string.IsNullOrEmpty(a))
            {
                return string.IsNullOrEmpty(b);
            }
            else
            {
                return string.Equals(a, b, comparison);
            }
        }

        public static bool IsNotEqualTo(this string? a, string? b, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            return !a.IsEqualTo(b, comparison);
        }

        public static bool IsWhiteSpace([NotNullWhen(false)] this string? input)
        {
            return string.IsNullOrWhiteSpace(input);
        }

        public static bool IsNotWhiteSpace([NotNullWhen(true)] this string? input)
        {
            return !string.IsNullOrWhiteSpace(input);
        }
    }
}
