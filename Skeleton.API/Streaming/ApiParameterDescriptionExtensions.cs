using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Skeleton.API.Streaming
{
    internal static class ApiParameterDescriptionExtensions
    {
        internal static bool IsFromForm(this ApiParameterDescription apiParameter)
        {
            var source = apiParameter.Source;
            var elementType = apiParameter.ModelMetadata?.ElementType;

            return source == BindingSource.Form || source == BindingSource.FormFile
                || elementType != null && typeof(IFormFile).IsAssignableFrom(elementType);
        }
    }
}
