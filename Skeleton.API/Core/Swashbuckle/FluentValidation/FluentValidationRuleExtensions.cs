using FluentValidation.Validators;
using Microsoft.OpenApi.Models;

namespace Skeleton.API.Core.Swashbuckle.FluentValidation
{
    /// <summary>
    /// Extensions for FluentValidationRules.
    /// </summary>
    public static class FluentValidationRuleExtensions
    {
        /// <summary>
        /// Overrides source rules with <paramref name="overrides"/> by name.
        /// </summary>
        /// <typeparam name="TSchema">Schema implementation.</typeparam>
        /// <param name="source">Source rules.</param>
        /// <param name="overrides">Overrides list.</param>
        /// <returns>New rule list.</returns>
        public static IReadOnlyList<IFluentValidationRule<TSchema>> OverrideRules<TSchema>(
            this IReadOnlyList<IFluentValidationRule<TSchema>> source,
            IEnumerable<IFluentValidationRule<TSchema>>? overrides)
        {
            if (overrides != null)
            {
                var validationRules = source.ToDictionary(rule => rule.Name, rule => rule);
                foreach (var validationRule in overrides)
                {
                    validationRules[validationRule.Name] = validationRule;
                }

                return [.. validationRules.Values];
            }

            return source;
        }

        /// <summary>
        /// Checks that validator is matches rule.
        /// </summary>
        /// <param name="rule">The rule.</param>
        /// <param name="validator">Validator.</param>
        /// <returns>True if validator matches rule.</returns>
        public static bool IsMatches(this IFluentValidationRule rule, IPropertyValidator validator)
        {
            foreach (var match in rule.Conditions)
            {
                if (!match(validator))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Adds match predicate.
        /// </summary>
        /// <param name="rule">Source rule.</param>
        /// <param name="validatorPredicate">Validator selector.</param>
        /// <returns>New rule instance.</returns>
        public static FluentValidationRule WithCondition(this FluentValidationRule rule, Func<IPropertyValidator, bool> validatorPredicate)
        {
            var matches = rule.Conditions.Append(validatorPredicate).ToArray();
            return new FluentValidationRule(rule.Name, matches, rule.Apply);
        }

        /// <summary>
        /// Sets <see cref="FluentValidationRule.Apply"/> action.
        /// </summary>
        /// <param name="rule">Source rule.</param>
        /// <param name="applyAction">New apply action.</param>
        /// <returns>New rule instance.</returns>
        public static FluentValidationRule WithApply(this FluentValidationRule rule, Action<IRuleContext<OpenApiSchema>> applyAction)
        {
            return new FluentValidationRule(rule.Name, rule.Conditions, applyAction);
        }
    }
}