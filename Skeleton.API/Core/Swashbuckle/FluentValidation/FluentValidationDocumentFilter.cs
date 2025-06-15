﻿using FluentValidation;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Skeleton.API.Core.Swashbuckle.FluentValidation.Core;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Skeleton.API.Core.Swashbuckle.FluentValidation
{
    /// <summary>
    /// Experimental document filter.
    /// </summary>
    public class FluentValidationDocumentFilter : IDocumentFilter
    {
        private readonly ILogger _logger;
        private readonly IValidatorRegistry _validatorRegistry;
        private readonly IReadOnlyList<IFluentValidationRule<OpenApiSchema>> _rules;
        private readonly SchemaGenerationOptions _schemaGenerationOptions;

        public FluentValidationDocumentFilter(
            /* System services */
            ILoggerFactory loggerFactory,
            IServiceProvider serviceProvider,

            /* MicroElements services */
            IValidatorRegistry validatorRegistry,
            IEnumerable<FluentValidationRule>? rules = null,
            IOptions<SchemaGenerationOptions>? schemaGenerationOptions = null)
        {
            // System services
            _logger = loggerFactory.CreateLogger<FluentValidationRules>();

            _logger.LogDebug("FluentValidationRules Created");

            _validatorRegistry = validatorRegistry;
            _rules = new DefaultFluentValidationRuleProvider(schemaGenerationOptions).GetRules().ToArray().OverrideRules(rules);
            _schemaGenerationOptions = schemaGenerationOptions?.Value ?? new SchemaGenerationOptions();
        }

        record SchemaItem
        {
            public Type ModelType { get; init; }
            public string SchemaName { get; init; }
            public OpenApiSchema Schema { get; init; }
        }

        record ParameterItem
        {
            public ApiDescription ApiDescription { get; init; }
            public ApiParameterDescription ParameterDescription { get; init; }
            public Type ModelType { get; init; }
            public string SchemaName { get; init; }
            public OpenApiSchema Schema { get; init; }
            public OpenApiSchema ParameterSchema { get; init; }
        }

        /// <inheritdoc />
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            if (_schemaGenerationOptions.SchemaIdSelector is null)
            {
                throw new InvalidOperationException("SchemaIdSelector is not set. Use 'AddFluentValidationRulesToSwagger' to set it.");
            }

            var schemaRepositorySchemas = context.SchemaRepository.Schemas;
            var schemaIdSelector = _schemaGenerationOptions.SchemaIdSelector;
            SwashbuckleSchemaProvider schemaProvider = new(context.SchemaRepository, context.SchemaGenerator, schemaIdSelector);

            ApiDescription[] apiDescriptions = [.. context.ApiDescriptions];

            IEnumerable<Type> modelTypes = apiDescriptions
                .SelectMany(description => description.ParameterDescriptions)
                .Where(description => description.ModelMetadata.ContainerType is null)
                .Select(description => description.ModelMetadata.ModelType)
                .Distinct();

            IEnumerable<Type> containerTypes = apiDescriptions
                .SelectMany(description => description.ParameterDescriptions)
                .Where(description => description.ModelMetadata.ContainerType is not null)
                .Select(description => description.ModelMetadata.ContainerType!)
                .Distinct();

            SchemaItem[] schemasForTypes = [.. modelTypes
                .Concat(containerTypes)
                .Distinct()
                .Select(modelType => new SchemaItem
                {
                    ModelType = modelType,
                    SchemaName = schemaIdSelector.Invoke(modelType),
                    Schema = schemaProvider.GetSchemaForType(modelType)
                })];

            ParameterItem[] schemasForParameters = [.. apiDescriptions
                .SelectMany(description => description.ParameterDescriptions)
                .Where(description => description.ModelMetadata.ContainerType is not null)
                .Select(description => new ParameterItem
                {
                    ParameterDescription = description,
                    ModelType = description.ModelMetadata.ContainerType!,
                    SchemaName = schemaIdSelector.Invoke(description.ModelMetadata.ContainerType!),
                    Schema = schemaProvider.GetSchemaForType(description.ModelMetadata.ContainerType!)
                })];

            IEnumerable<ParameterItem> GetParameters()
            {
                foreach (ApiDescription apiDescription in apiDescriptions)
                {
                    foreach (ApiParameterDescription apiParameterDescription in apiDescription.ParameterDescriptions)
                    {
                        Type? containerType = apiParameterDescription.ModelMetadata.ContainerType;
                        if (containerType is not null)
                        {
                            ParameterItem parameterItem = new()
                            {
                                ApiDescription = apiDescription,
                                ParameterDescription = apiParameterDescription,
                                ModelType = containerType,
                                SchemaName = schemaIdSelector.Invoke(containerType),
                                Schema = schemaProvider.GetSchemaForType(containerType),
                            };

                            OpenApiSchema parameterSchema = FindParam(parameterItem);
                            parameterItem = parameterItem with { ParameterSchema = parameterSchema };

                            yield return parameterItem;
                        }
                    }
                }
            }

            schemasForParameters = [.. GetParameters()];

            OpenApiSchema FindParam(ParameterItem item)
            {
                //return many?
                var path = swaggerDoc.Paths.FirstOrDefault(pair => pair.Key.TrimStart('/') == item.ApiDescription.RelativePath);
                var openApiParameter = path.Value.Operations.Values.First().Parameters.First(parameter => parameter.Name == item.ParameterDescription.Name);
                return openApiParameter.Schema;
            }

            foreach (SchemaItem item in schemasForTypes)
            {
                IValidator? validator = null;
                try
                {
                    validator = _validatorRegistry.GetValidator(item.ModelType);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(0, e, "GetValidator for type '{ModelType}' fails.", item.ModelType);
                }

                if (validator == null)
                {
                    continue;
                }

                TypeContext typeContext = new(item.ModelType, _schemaGenerationOptions);
                ValidatorContext validatorContext = new(typeContext, validator);

                SchemaGenerationContext schemaContext = new(
                    schemaRepository: context.SchemaRepository,
                    schemaGenerator: context.SchemaGenerator,
                    schema: item.Schema,
                    schemaType: item.ModelType,
                    rules: _rules,
                    schemaGenerationOptions: _schemaGenerationOptions);

                ApplyRulesToSchema(schemaContext, validator);

                try
                {
                    AddRulesFromIncludedValidators(schemaContext, validatorContext);
                }
                catch (Exception e)
                {
                    //TODO: functional
                    _logger.LogWarning(0, e, "Applying IncludeRules for type '{ModelType}' fails.", item.ModelType);
                }
            }

            foreach (var item in schemasForParameters)
            {
                var itemParameterDescription = item.ParameterDescription;
                var schemaPropertyName = itemParameterDescription.ModelMetadata.BinderModelName ?? itemParameterDescription.Name;
                var parameterSchema = item.ParameterSchema;
                var schema = item.Schema;
                if (schema.Properties.TryGetValue(schemaPropertyName.ToLowerCamelCase(), out var property)
                    || schema.Properties.TryGetValue(schemaPropertyName, out property))
                {
                    // Copy from property schema to parameter schema.
                    parameterSchema.Description = property.Description;
                    parameterSchema.MinLength = property.MinLength;
                    parameterSchema.Nullable = property.Nullable;
                    parameterSchema.MaxLength = property.MaxLength;
                    parameterSchema.Pattern = property.Pattern;
                    parameterSchema.Minimum = property.Minimum;
                    parameterSchema.Maximum = property.Maximum;
                    parameterSchema.ExclusiveMaximum = property.ExclusiveMaximum;
                    parameterSchema.ExclusiveMinimum = property.ExclusiveMinimum;
                    parameterSchema.Enum = property.Enum;
                    parameterSchema.AllOf = property.AllOf;
                }
            }
        }

        private void ApplyRulesToSchema(SchemaGenerationContext schemaGenerationContext, IValidator validator)
        {
            FluentValidationSchemaBuilder.ApplyRulesToSchema(
                schemaType: schemaGenerationContext.SchemaType,
                schemaPropertyNames: schemaGenerationContext.Properties,
                validator: validator,
                logger: _logger,
                schemaGenerationContext: schemaGenerationContext);
        }

        [Obsolete("There is a repeat")]
        private void AddRulesFromIncludedValidators(SchemaGenerationContext schemaGenerationContext, ValidatorContext validatorContext)
        {
            FluentValidationSchemaBuilder.AddRulesFromIncludedValidators(
                validatorContext: validatorContext,
                logger: _logger,
                schemaGenerationContext: schemaGenerationContext);
        }
    }
}