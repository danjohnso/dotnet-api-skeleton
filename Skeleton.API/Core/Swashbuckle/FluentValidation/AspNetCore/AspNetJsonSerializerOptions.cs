using System.Text.Json;

namespace Skeleton.API.Core.Swashbuckle.FluentValidation.AspNetCore
{
    /// <summary>
    /// AspNetCore Mvc wrapper that can be used in netstandard.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AspNetJsonSerializerOptions"/> class.
    /// </remarks>
    /// <param name="value"><see cref="JsonSerializerOptions"/> from AspNet host.</param>
    public class AspNetJsonSerializerOptions(JsonSerializerOptions value)
    {

        /// <summary>
        /// Gets the <see cref="JsonSerializerOptions"/> from AspNet host.
        /// </summary>
        public JsonSerializerOptions Value { get; } = value;
    }
}