﻿using FluentValidation.Validators;

namespace Skeleton.API.Core.Swashbuckle.FluentValidation
{
    /// <summary>
    /// FluentValidationRule.
    /// </summary>
    public interface IFluentValidationRule
    {
        /// <summary>
        /// Gets the rule name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets predicates that checks the validator is matches the rule.
        /// </summary>
        IReadOnlyCollection<Func<IPropertyValidator, bool>> Conditions { get; }
    }

    /// <summary>
    /// Generic FluentValidationRule. Knows how to modify OpenApi schema.
    /// </summary>
    /// <typeparam name="TSchema">OpenApi implementation.</typeparam>
    public interface IFluentValidationRule<in TSchema> : IFluentValidationRule
    {
        /// <summary>
        /// Gets the action that modifies OpenApi schema.
        /// </summary>
        public Action<IRuleContext<TSchema>>? Apply { get; }
    }
}