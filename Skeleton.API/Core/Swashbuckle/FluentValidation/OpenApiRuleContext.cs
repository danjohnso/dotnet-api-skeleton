using FluentValidation;
using FluentValidation.Validators;
using Microsoft.OpenApi.Models;

namespace Skeleton.API.Core.Swashbuckle.FluentValidation
{
    /// <summary>
    /// RuleContext.
    /// </summary>
    public class OpenApiRuleContext(OpenApiSchema schema, string propertyKey, ValidationRuleContext validationRuleInfo, IPropertyValidator propertyValidator) : IRuleContext<OpenApiSchema>
    {
        /// <summary>
        /// Gets property name in schema.
        /// </summary>
        public string PropertyKey { get; } = propertyKey;

        /// <summary>
        /// Gets property validator for property in schema.
        /// </summary>
        public IPropertyValidator PropertyValidator { get; } = propertyValidator;

        /// <summary>
        /// Gets OpenApi (swagger) schema.
        /// </summary>
        public OpenApiSchema Schema { get; } = schema;

        /// <summary>
        /// Gets target property schema.
        /// </summary>
        public OpenApiSchema Property
        {
            get
            {
                if (!Schema.Properties.TryGetValue(PropertyKey, out OpenApiSchema? schemaProperty))
                {
                    Type? schemaType = _validationRuleInfo.GetReflectionContext()?.Type;
                    throw new ApplicationException($"Schema for type '{schemaType}' does not contain property '{PropertyKey}'.\nRegister {typeof(INameResolver)} if name in type differs from name in json.");
                }

                return !_validationRuleInfo.IsCollectionRule() ? schemaProperty : schemaProperty.Items;
            }
        }

        private readonly ValidationRuleContext _validationRuleInfo = validationRuleInfo;
    }
}