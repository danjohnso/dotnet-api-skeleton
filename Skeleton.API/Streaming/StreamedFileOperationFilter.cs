using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Skeleton.API.Streaming
{
    public class StreamedFileOperation : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            bool isStreamedFileUploadOperation = context.MethodInfo.CustomAttributes.Any(a => a.AttributeType == typeof(DisableFormValueModelBindingAttribute));
            if (isStreamedFileUploadOperation)
            {
                List<ApiParameterDescription> formParameters = [.. context.ApiDescription.ParameterDescriptions.Where(paramDesc => paramDesc.IsFromForm())];

                OpenApiRequestBody requestBody = GenerateRequestBodyFromFormParameters(context.ApiDescription, context.SchemaRepository, context.SchemaGenerator, formParameters);

                requestBody.Content["multipart/form-data"].Schema.Properties.Add("file", new()
                {
                    Description = "Upload File",
                    Type = "file",
                    Format = "binary"
                });

                requestBody.Content["multipart/form-data"].Schema.Required.Add("file");

                operation.RequestBody = requestBody;
            }
        }

        private static OpenApiRequestBody GenerateRequestBodyFromFormParameters(ApiDescription apiDescription, SchemaRepository schemaRepository, ISchemaGenerator schemaGenerator, IEnumerable<ApiParameterDescription> formParameters)
        {
            string[] contentTypes = ["multipart/form-data"];

            OpenApiSchema schema = GenerateSchemaFromFormParameters(formParameters, schemaRepository, schemaGenerator);

            return new OpenApiRequestBody
            {
                Content = contentTypes.ToDictionary(
                    contentType => contentType,
                    contentType => new OpenApiMediaType
                    {
                        Schema = schema,
                        Encoding = schema.Properties.ToDictionary(
                            entry => entry.Key,
                            entry => new OpenApiEncoding { Style = ParameterStyle.Form }
                        )
                    }
                )
            };
        }

        private static OpenApiSchema GenerateSchemaFromFormParameters(IEnumerable<ApiParameterDescription> formParameters, SchemaRepository schemaRepository, ISchemaGenerator schemaGenerator)
        {
            Dictionary<string, OpenApiSchema> properties = [];
            List<string> requiredPropertyNames = [];

            foreach (ApiParameterDescription formParameter in formParameters)
            {
                OpenApiSchema schema = formParameter.ModelMetadata != null
                    ? GenerateSchema(formParameter.ModelMetadata.ModelType, schemaRepository, schemaGenerator, formParameter.PropertyInfo(), formParameter.ParameterInfo())
                    : new OpenApiSchema { Type = "string" };

                properties.Add(formParameter.Name, schema);

                if (formParameter.IsRequiredParameter())
                {
                    requiredPropertyNames.Add(formParameter.Name);
                }
            }

            return new OpenApiSchema
            {
                Type = "object",
                Properties = properties,
                Required = new SortedSet<string>(requiredPropertyNames)
            };
        }

        private static OpenApiSchema GenerateSchema(Type type, SchemaRepository schemaRepository, ISchemaGenerator schemaGenerator, PropertyInfo? propertyInfo = null, ParameterInfo? parameterInfo = null, ApiParameterRouteInfo? routeInfo = null)
        {
            try
            {
                return schemaGenerator.GenerateSchema(type, schemaRepository, propertyInfo, parameterInfo, routeInfo);
            }
            catch (Exception ex)
            {
                throw new SwaggerGeneratorException(message: $"Failed to generate schema for type - {type}. See inner exception", innerException: ex);
            }
        }
    }
}
