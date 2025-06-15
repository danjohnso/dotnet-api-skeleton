﻿using FluentValidation.Validators;
using Skeleton.API.Core.Swashbuckle.FluentValidation;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Skeleton.API.Core.Swashbuckle.FluentValidation.Core;

namespace Skeleton.API.Core.Swashbuckle.FluentValidation
{
    /// <summary>
    /// Default rule provider.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="DefaultFluentValidationRuleProvider"/> class.
    /// </remarks>
    /// <param name="options">Schema generation options.</param>
    public class DefaultFluentValidationRuleProvider(IOptions<SchemaGenerationOptions>? options = null) : IFluentValidationRuleProvider<OpenApiSchema>
    {
        private readonly IOptions<SchemaGenerationOptions> _options = options ?? new OptionsWrapper<SchemaGenerationOptions>(new SchemaGenerationOptions());

        /// <inheritdoc />
        public IEnumerable<IFluentValidationRule<OpenApiSchema>> GetRules()
        {
            yield return new FluentValidationRule("Required")
                .WithCondition(validator => validator is INotNullValidator || validator is INotEmptyValidator)
                .WithApply(context =>
                {
                    if (!context.Schema.Required.Contains(context.PropertyKey))
                        context.Schema.Required.Add(context.PropertyKey);

                    context.Property.Nullable = false;
                });

            yield return new FluentValidationRule("NotEmpty")
                .WithCondition(validator => validator is INotEmptyValidator)
                .WithApply(context =>
                {
                    if (context.Property.Type == "string")
                        context.Property.SetNewMin(p => p.MinLength, 1, _options.Value.SetNotNullableIfMinLengthGreaterThenZero);

                    if (context.Property.Type == "array")
                        context.Property.SetNewMin(p => p.MinItems, 1, _options.Value.SetNotNullableIfMinLengthGreaterThenZero);
                });

            yield return new FluentValidationRule("Length")
                .WithCondition(validator => validator is ILengthValidator)
                .WithApply(context =>
                {
                    var lengthValidator = (ILengthValidator) context.PropertyValidator;
                    var schemaProperty = context.Property;

                    if (schemaProperty.Type == "array")
                    {
                        if (lengthValidator.Max > 0)
                            schemaProperty.SetNewMax(p => p.MaxItems, lengthValidator.Max);

                        if (lengthValidator.Min > 0)
                            schemaProperty.SetNewMin(p => p.MinItems, lengthValidator.Min, _options.Value.SetNotNullableIfMinLengthGreaterThenZero);
                    }
                    else
                    {
                        if (lengthValidator.Max > 0)
                            schemaProperty.SetNewMax(p => p.MaxLength, lengthValidator.Max);

                        if (lengthValidator.Min > 0)
                            schemaProperty.SetNewMin(p => p.MinLength, lengthValidator.Min, _options.Value.SetNotNullableIfMinLengthGreaterThenZero);
                    }
                });

            yield return new FluentValidationRule("Pattern")
                .WithCondition(validator => validator is IRegularExpressionValidator)
                .WithApply(context =>
                {
                    var regularExpressionValidator = (IRegularExpressionValidator) context.PropertyValidator;
                    var schemaProperty = context.Property;

                    if (_options.Value.UseAllOfForMultipleRules)
                    {
                        if (schemaProperty.Pattern != null ||
                            schemaProperty.AllOf.Any(schema => schema.Pattern != null))
                        {
                            if (!schemaProperty.AllOf.Any(schema => schema.Pattern != null))
                            {
                                // Add first pattern as AllOf
                                schemaProperty.AllOf.Add(new OpenApiSchema()
                                {
                                    Pattern = schemaProperty.Pattern,
                                });
                            }

                            // Add another pattern as AllOf
                            schemaProperty.AllOf.Add(new OpenApiSchema()
                            {
                                Pattern = regularExpressionValidator.Expression,
                            });

                            schemaProperty.Pattern = null;
                        }
                        else
                        {
                            // First and only pattern
                            schemaProperty.Pattern = regularExpressionValidator.Expression;
                        }
                    }
                    else
                    {
                        // Set new pattern
                        schemaProperty.Pattern = regularExpressionValidator.Expression;
                    }
                });

            yield return new FluentValidationRule("EMail")
                .WithCondition(propertyValidator => propertyValidator.GetType().Name.Contains("EmailValidator"))
                .WithApply(context => context.Property.Format = "email");

            yield return new FluentValidationRule("Comparison")
                .WithCondition(validator => validator is IComparisonValidator)
                .WithApply(context =>
                {
                    var comparisonValidator = (IComparisonValidator)context.PropertyValidator;
                    if (comparisonValidator.ValueToCompare.IsNumeric())
                    {
                        var valueToCompare = comparisonValidator.ValueToCompare.NumericToDecimal();
                        var schemaProperty = context.Property;

                        if (comparisonValidator.Comparison == Comparison.GreaterThanOrEqual)
                        {
                            schemaProperty.SetNewMin(p => p.Minimum, valueToCompare, _options.Value.SetNotNullableIfMinLengthGreaterThenZero);
                        }
                        else if (comparisonValidator.Comparison == Comparison.GreaterThan)
                        {
                            schemaProperty.SetNewMin(p => p.Minimum, valueToCompare, _options.Value.SetNotNullableIfMinLengthGreaterThenZero);
                            schemaProperty.ExclusiveMinimum = true;
                        }
                        else if (comparisonValidator.Comparison == Comparison.LessThanOrEqual)
                        {
                            schemaProperty.SetNewMax(p => p.Maximum, valueToCompare);
                        }
                        else if (comparisonValidator.Comparison == Comparison.LessThan)
                        {
                            schemaProperty.SetNewMax(p => p.Maximum, valueToCompare);
                            schemaProperty.ExclusiveMaximum = true;
                        }
                    }
                });

            yield return new FluentValidationRule("Between")
                .WithCondition(validator => validator is IBetweenValidator)
                .WithApply(context =>
                {
                    IBetweenValidator betweenValidator = (IBetweenValidator)context.PropertyValidator;
                    var schemaProperty = context.Property;

                    //OpenApi: date-time has not support range validations see: https://github.com/json-schema-org/json-schema-spec/issues/116

                    if (betweenValidator.From.IsNumeric())
                    {
                        schemaProperty.SetNewMin(p => p.Minimum, betweenValidator.From.NumericToDecimal(), _options.Value.SetNotNullableIfMinLengthGreaterThenZero);

                        if (betweenValidator.Name == "ExclusiveBetweenValidator")
                        {
                            schemaProperty.ExclusiveMinimum = true;
                        }
                    }

                    if (betweenValidator.To.IsNumeric())
                    {
                        schemaProperty.SetNewMax(p => p.Maximum, betweenValidator.To.NumericToDecimal());

                        if (betweenValidator.Name == "ExclusiveBetweenValidator")
                        {
                            schemaProperty.ExclusiveMaximum = true;
                        }
                    }
                });
        }
    }
}