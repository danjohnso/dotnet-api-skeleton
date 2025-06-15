using System.Reflection;

namespace Skeleton.API.Core.Swashbuckle.FluentValidation
{
    /// <summary>
    /// Name resolver.
    /// Gets property name using naming conventions.
    /// </summary>
    public interface INameResolver
    {
        /// <summary>
        /// Gets schema name for property.
        /// </summary>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Property schema name.</returns>
        string GetPropertyName(PropertyInfo propertyInfo);
    }
}