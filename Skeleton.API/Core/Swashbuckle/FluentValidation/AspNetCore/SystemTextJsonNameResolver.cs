using Skeleton.API.Core.Swashbuckle.FluentValidation;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Skeleton.API.Core.Swashbuckle.FluentValidation.AspNetCore
{
    /// <summary>
    /// Resolves name according System.Text.Json <see cref="JsonPropertyNameAttribute"/> or <see cref="JsonSerializerOptions.PropertyNamingPolicy"/>.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="SystemTextJsonNameResolver"/> class.
    /// </remarks>
    /// <param name="serializerOptions"><see cref="JsonSerializerOptions"/>.</param>
    public class SystemTextJsonNameResolver(AspNetJsonSerializerOptions? serializerOptions = null) : INameResolver
    {
        private readonly JsonSerializerOptions? _serializerOptions = serializerOptions?.Value ?? new JsonSerializerOptions();

        /// <inheritdoc />
        public string GetPropertyName(PropertyInfo propertyInfo)
        {
            if (propertyInfo.GetCustomAttribute<JsonPropertyNameAttribute>() is { Name: { } jsonPropertyName })
            {
                return jsonPropertyName;
            }

            if (_serializerOptions?.PropertyNamingPolicy is { } jsonNamingPolicy)
            {
                return jsonNamingPolicy.ConvertName(propertyInfo.Name);
            }

            return propertyInfo.Name;
        }
    }
}