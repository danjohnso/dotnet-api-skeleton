﻿using FluentValidation.Validators;

namespace Skeleton.API.Core.Swashbuckle.FluentValidation
{
    /// <summary>
    /// Represents context for schema generation.
    /// </summary>
    public interface ISchemaGenerationContext
    {
        /// <summary>
        /// Gets the schema .net type.
        /// </summary>
        Type SchemaType { get; }

        /// <summary>
        /// Gets the schema properties.
        /// </summary>
        IEnumerable<string> Properties { get; }

        /// <summary>
        /// Gets the validation rules to apply.
        /// </summary>
        IReadOnlyList<IFluentValidationRule> Rules { get; }

        /// <summary>
        /// Gets <see cref="ISchemaGenerationOptions"/>.
        /// </summary>
        ISchemaGenerationOptions SchemaGenerationOptions { get; }
    }

    /// <summary>
    /// Represents context typed with schema implementation for schema generation.
    /// </summary>
    public interface ISchemaGenerationContext<TSchema> : ISchemaGenerationContext
    {
        /// <summary>
        /// Gets OpenApi schema.
        /// </summary>
        TSchema Schema { get; }

        /// <summary>
        /// Gets schema provider.
        /// </summary>
        ISchemaProvider<TSchema> SchemaProvider { get; }

        /// <summary>
        /// Gets the validation rules to apply.
        /// </summary>
        new IReadOnlyList<IFluentValidationRule<TSchema>> Rules { get; }

        /// <summary>
        /// Gets the context copy with other schema.
        /// </summary>
        /// <param name="schema">The new schema.</param>
        /// <returns>The context copy.</returns>
        ISchemaGenerationContext<TSchema> With(TSchema schema);

        /// <summary>
        /// Creates concrete rule context.
        /// </summary>
        /// <param name="schemaPropertyName">The property name.</param>
        /// <param name="validationRuleContext">Validation rule context.</param>
        /// <param name="propertyValidator">Validator.</param>
        /// <returns>New rule context.</returns>
        IRuleContext<TSchema> Create(string schemaPropertyName, ValidationRuleContext validationRuleContext, IPropertyValidator propertyValidator);
    }
}