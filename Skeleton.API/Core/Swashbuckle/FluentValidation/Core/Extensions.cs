namespace Skeleton.API.Core.Swashbuckle.FluentValidation.Core
{
    /// <summary>
    /// Extensions for some swagger specific work.
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Is supported swagger numeric type.
        /// </summary>
        internal static bool IsNumeric(this object value) => value is int || value is long || value is float || value is double || value is decimal;

        /// <summary>
        /// Convert numeric to double.
        /// </summary>
        internal static decimal NumericToDecimal(this object value) => Convert.ToDecimal(value);

        /// <summary>
        /// Returns not null enumeration.
        /// </summary>
        internal static IEnumerable<TValue> NotNull<TValue>(this IEnumerable<TValue>? collection) => collection ?? [];

        /// <summary>
        /// Skip simple types from schema generation.
        /// </summary>
        internal static bool IsPrimitiveType(this Type type) => type.IsPrimitive || type == typeof(string) || type == typeof(decimal);

        /// <summary>
        /// Returns array in debug mode and the same collection in release.
        /// </summary>
        internal static IEnumerable<TValue> ToArrayDebug<TValue>(this IEnumerable<TValue>? collection)
        {
#if DEBUG
            return collection?.ToArray() ?? [];
#else
            return collection;
#endif
        }
    }
}