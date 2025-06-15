using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;

namespace Skeleton.API.Core.Swagger
{
    internal class ConfigureSwaggerGenOptions(IApiVersionDescriptionProvider descriptionProvider, IOptions<SwaggerConfigurationOptions> options) : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _descriptionProvider = descriptionProvider;
        private readonly SwaggerConfigurationOptions _options = options.Value;

        public void Configure(SwaggerGenOptions options)
        {
            options.UseAllOfToExtendReferenceSchemas();

            // to add bearer token in Swagger
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                BearerFormat = "JWT",
                Description = "Please insert JWT token into the field below",
                In = ParameterLocation.Header,
                Scheme = "bearer",
                Type = SecuritySchemeType.Http
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            string apiSpecDescription = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), _options.SpecificationFileName), Encoding.UTF8);

            // add swagger document for every API version discovered
            foreach (ApiVersionDescription description in _descriptionProvider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName.ToLower(), new()
                {
                    Title = _options.ApiName,
                    Version = description.GroupName.ToLower(),
                    Description = $"{(description.IsDeprecated ? "### *DEPRECATED*\n" : string.Empty)}{apiSpecDescription}"
                });
            }

            // add all available xml documentation files to Swagger (i.e. API, Model, etc.)
            List<string> xmlFiles = [.. Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly)];
            xmlFiles.ForEach(xmlFile => options.IncludeXmlComments(xmlFile));
        }
    }
}
