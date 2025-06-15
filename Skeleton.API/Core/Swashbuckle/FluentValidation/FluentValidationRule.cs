using FluentValidation.Validators;
using Microsoft.OpenApi.Models;

namespace Skeleton.API.Core.Swashbuckle.FluentValidation
{
    /// <summary>
    /// FluentValidationRule.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="FluentValidationRule"/> class.
    /// </remarks>
    /// <param name="name">Rule name.</param>
    /// <param name="matches">Validator predicates.</param>
    /// <param name="apply">Apply rule to schema action.</param>
    public class FluentValidationRule(
        string name,
        IReadOnlyCollection<Func<IPropertyValidator, bool>>? matches = null,
        Action<IRuleContext<OpenApiSchema>>? apply = null) : IFluentValidationRule<OpenApiSchema>
    {
        /// <summary>
        /// Gets rule name.
        /// </summary>
        public string Name { get; } = name;

        /// <summary>
        /// Gets predicates that checks validator is matches rule.
        /// </summary>
        public IReadOnlyCollection<Func<IPropertyValidator, bool>> Conditions { get; } = matches ?? [];

        /// <summary>
        /// Gets action that modifies swagger schema.
        /// </summary>
        public Action<IRuleContext<OpenApiSchema>>? Apply { get; } = apply;
    }
}