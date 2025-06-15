namespace Skeleton.Core.Extensions
{
    public static class StringExtensions
    {
        public static bool IsWhiteSpace(this string input)
        {
            return string.IsNullOrWhiteSpace(input);
        }

        public static bool IsNotWhiteSpace(this string input)
        {
            return !string.IsNullOrWhiteSpace(input);
        }
    }
}
