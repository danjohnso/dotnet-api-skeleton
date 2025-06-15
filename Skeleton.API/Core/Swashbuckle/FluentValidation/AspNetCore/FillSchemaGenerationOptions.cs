using Skeleton.API.Core.Swashbuckle.FluentValidation.Core;
using Microsoft.Extensions.Options;
using Skeleton.API.Core.Swashbuckle.FluentValidation;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Skeleton.API.Core.Swashbuckle.FluentValidation.AspNetCore
{
    /// <summary>
    /// Fills <see cref="SchemaGenerationOptions"/> default values on PostConfigure action.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="FillSchemaGenerationOptions"/> class.
    /// </remarks>
    /// <param name="serviceProvider">The source service provider.</param>
    /// <param name="swaggerGenOptions">Swashbuckle options.</param>
    public class FillSchemaGenerationOptions(IServiceProvider serviceProvider, IOptions<SwaggerGenOptions>? swaggerGenOptions = null) : IPostConfigureOptions<SchemaGenerationOptions>
    {
        /// <inheritdoc />
        public void PostConfigure(string? name, SchemaGenerationOptions options)
        {
            // Fills options from SwashbuckleOptions
            options.SchemaIdSelector ??= swaggerGenOptions?.Value.SchemaGeneratorOptions.SchemaIdSelector ?? new SchemaGeneratorOptions().SchemaIdSelector;

            // Assume that all needed is filled
            options.NameResolver ??= serviceProvider?.GetService<IServicesContext>()?.NameResolver;
            options.ValidatorSearch ??= ValidatorSearchSettings.Default;
            options.ValidatorFilter ??= new Condition<ValidatorContext>(context => context.Validator.CanValidateInstancesOfType(context.TypeContext.TypeToValidate));
            options.RuleFilter ??= new Condition<ValidationRuleContext>(context => context.ValidationRule.HasNoCondition());
            options.RuleComponentFilter ??= new Condition<RuleComponentContext>(context => context.RuleComponent.HasNoCondition());
        }
    }
}