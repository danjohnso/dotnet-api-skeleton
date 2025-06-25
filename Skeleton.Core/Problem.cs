using System.Text.Json.Serialization;

namespace Skeleton.Core
{
    public record Problem([property: JsonPropertyName("code")] string Code, [property: JsonPropertyName("message")] string Message);
}