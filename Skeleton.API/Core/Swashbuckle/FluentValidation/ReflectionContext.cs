using System.Reflection;

namespace Skeleton.API.Core.Swashbuckle.FluentValidation
{
    /// <summary>
    /// Reflection context for <see cref="RuleContext"/>.
    /// </summary>
    public class ReflectionContext(Type? type = null, MemberInfo? propertyInfo = null)
    {
        /// <summary>
        /// Gets the type (schema type).
        /// </summary>
        public Type? Type { get; } = type;

        /// <summary>
        /// Gets optional PropertyInfo.
        /// </summary>
        public MemberInfo? PropertyInfo { get; } = propertyInfo;
    }
}