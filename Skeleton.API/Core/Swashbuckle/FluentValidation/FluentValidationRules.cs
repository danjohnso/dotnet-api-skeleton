﻿using FluentValidation;
using Skeleton.API.Core.Swashbuckle.FluentValidation.ValidatorRegistry;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Skeleton.API.Core.Swashbuckle.FluentValidation.Core;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Skeleton.API.Core.Swashbuckle.FluentValidation
{
    /// <summary>
    /// Swagger <see cref="ISchemaFilter"/> that uses FluentValidation validators instead System.ComponentModel based attributes.
    /// </summary>
    public class FluentValidationRules : ISchemaFilter
    {
        private readonly ILogger _logger;

        private readonly IValidatorRegistry _validatorRegistry;

        private readonly IReadOnlyList<IFluentValidationRule<OpenApiSchema>> _rules;
        private readonly SchemaGenerationOptions _schemaGenerationOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentValidationRules"/> class.
        /// </summary>
        /// <param name="loggerFactory"><see cref="ILoggerFactory"/> for logging. Can be null.</param>
        /// <param name="serviceProvider">Validator factory.</param>
        /// <param name="validatorRegistry">Gets validators for a particular type.</param>
        /// <param name="fluentValidationRuleProvider">Rules provider.</param>
        /// <param name="rules">External FluentValidation rules. External rule overrides default rule with the same name.</param>
        /// <param name="schemaGenerationOptions">Schema generation options.</param>
        public FluentValidationRules(
            /* System services */
            ILoggerFactory loggerFactory,
            IServiceProvider serviceProvider,

            /* MicroElements services */
            IValidatorRegistry? validatorRegistry = null,
            IFluentValidationRuleProvider<OpenApiSchema>? fluentValidationRuleProvider = null,
            IEnumerable<FluentValidationRule>? rules = null,
            IOptions<SchemaGenerationOptions>? schemaGenerationOptions = null)
        {
            // System services
            _logger = loggerFactory.CreateLogger<FluentValidationRules>();

            // FluentValidation services
            _validatorRegistry = validatorRegistry ?? new ServiceProviderValidatorRegistry(serviceProvider, schemaGenerationOptions);

            // MicroElements services
            fluentValidationRuleProvider ??= new DefaultFluentValidationRuleProvider(schemaGenerationOptions);
            _rules = fluentValidationRuleProvider.GetRules().ToArray().OverrideRules(rules);
            _schemaGenerationOptions = schemaGenerationOptions?.Value ?? new SchemaGenerationOptions();

            _logger.LogDebug("FluentValidationRules Created");
        }

        /// <inheritdoc />
        public void Apply(OpenApiSchema? schema, SchemaFilterContext context)
        {
            if (schema is null)
                return;

            // Do not process simple types.
            if (context.Type.IsPrimitiveType())
                return;

            var typeContext = new TypeContext(context.Type, _schemaGenerationOptions);

            var (validators, _) = Functional
                .Try(() => _validatorRegistry.GetValidators(context.Type).ToArray())
                .OnError(e => _logger.LogWarning(0, e, "GetValidators for type '{ModelType}' failed", context.Type));

            if (validators == null)
                return;

            var allSchemas = new List<OpenApiSchema>();
            ProcessAllSchemas(schema, allSchemas);

            foreach (var validator in validators)
            {
                foreach (var oneOfSchemas in allSchemas)
                {
                    var validatorContext = new ValidatorContext(typeContext, validator);
                    var schemaContext = new SchemaGenerationContext(
                        schemaRepository: context.SchemaRepository,
                        schemaGenerator: context.SchemaGenerator,
                        schemaType: context.Type,
                        schema: oneOfSchemas,
                        rules: _rules,
                        schemaGenerationOptions: _schemaGenerationOptions);

                    ApplyRulesToSchema(schemaContext, validator);

                    try
                    {
                        AddRulesFromIncludedValidators(schemaContext, validatorContext);
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning(0, e, "Applying IncludeRules for type '{ModelType}' failed", context.Type);
                    }
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

        private void AddRulesFromIncludedValidators(SchemaGenerationContext schemaGenerationContext, ValidatorContext validatorContext)
        {
            FluentValidationSchemaBuilder.AddRulesFromIncludedValidators(
                validatorContext: validatorContext,
                logger: _logger,
                schemaGenerationContext: schemaGenerationContext);
        }

        private static void ProcessAllSchemas(OpenApiSchema schema, List<OpenApiSchema> schemas)
        {
            schemas.Add(schema);

            IList<OpenApiSchema>[] collectionsOfSchemas = [schema.AllOf, schema.OneOf, schema.AnyOf];
            foreach (IList<OpenApiSchema> collectionOfSchemas in collectionsOfSchemas)
            {
                foreach (OpenApiSchema embededSchema in collectionOfSchemas)
                {
                    if (!embededSchema.Properties.Any())
                    {
                        continue;
                    }

                    schemas.Add(embededSchema);
                }
            }
        }
    }
}