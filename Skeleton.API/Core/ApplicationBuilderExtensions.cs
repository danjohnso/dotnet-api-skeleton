using Asp.Versioning.ApiExplorer;
using Microsoft.OpenApi.Models;
using Skeleton.API.Core;
using Skeleton.API.Core.Core;

namespace Skeleton.API.Core
{
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Registers a global exception handler
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseGlobalExceptionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionMiddleware>();
        }

        /// <summary>
        /// Adds Swagger UI and API to the application
        /// </summary>
        /// <param name="app"></param>
        /// <param name="apiVersionDescriptionProvider"></param>
        /// <returns></returns>
        public static void UseSwagger(this IApplicationBuilder app, IApiVersionDescriptionProvider apiVersionDescriptionProvider)
        {
            // enable middleware to serve generated Swagger as a JSON endpoint
            app.UseSwagger(_ =>
            {
                _.PreSerializeFilters.Add((swagger, httpRequest) =>
                {
                    swagger.Servers =
                    [
                        new()
                        {
                            // add the version segment to the server path
                            Url = $"{httpRequest.Scheme}://{httpRequest.Host.Value}/{swagger.Info.Version}"
                        }
                    ];

                    // remove the version segment from the operation paths because it's been added to the server path;
                    // this is done to ensure the API Spec is in a clean format for Azure APIM
                    OpenApiPaths newPaths = [];
                    foreach (KeyValuePair<string, OpenApiPathItem> path in swagger.Paths)
                    {
                        if (path.Key.StartsWith($"/{swagger.Info.Version}"))
                        {
                            newPaths.Add(path.Key.Replace($"/{swagger.Info.Version}", string.Empty), path.Value);
                        }
                    }

                    swagger.Paths = newPaths;
                });
            });

            // Enable middleware to serve swagger-ui, specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(_ =>
            {
                _.RoutePrefix = "";
                
                foreach (ApiVersionDescription description in apiVersionDescriptionProvider.ApiVersionDescriptions)
                {
                    _.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                }
            });
        }

        /// <summary>
        /// Adds /versions endpoint that reports on supported and deprecated API versions in the headers
        /// </summary>
        /// <param name="endpoints"></param>
        /// <param name="apiVersionDescriptionProvider"></param>
        /// <returns></returns>
        public static IEndpointConventionBuilder MapVersions(this IEndpointRouteBuilder endpoints, IApiVersionDescriptionProvider apiVersionDescriptionProvider)
        {
            //This attribute would normally report this, but it does not seem to work with "unversioned" minimal api endpoints so for now I am recreating this functionality manually
            //ReportApiVersionsAttribute
            string supportedVersions = string.Join(", ", apiVersionDescriptionProvider.ApiVersionDescriptions.Where(x => !x.IsDeprecated).Select(x => x.ApiVersion.MajorVersion.ToString()).ToArray());
            string deprecatedVersions = string.Join(", ", apiVersionDescriptionProvider.ApiVersionDescriptions.Where(x => x.IsDeprecated).Select(x => x.ApiVersion.MajorVersion.ToString()).ToArray());

            return endpoints.MapGet("/versions", (HttpContext httpContext) =>
            {
                if (supportedVersions.Length > 0)
                {
                    httpContext.Response.Headers.Append("api-supported-versions", supportedVersions);
                }

                if (deprecatedVersions.Length > 0)
                {
                    httpContext.Response.Headers.Append("api-deprecated-versions", deprecatedVersions);
                }

                return Results.Ok();
            });
        }
    }
}
