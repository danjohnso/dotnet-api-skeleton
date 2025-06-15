﻿using FluentValidation;

namespace Skeleton.API.Core.Swashbuckle.FluentValidation
{
    /// <summary>
    /// Gets validators for a particular type.
    /// </summary>
    public interface IValidatorRegistry
    {
        /// <summary>
        /// Gets the validator for the specified type.
        /// </summary>
        /// <param name="type">Type to validate.</param>
        /// <returns>Validator or <see langword="null"/>.</returns>
        IValidator? GetValidator(Type type);

        /// <summary>
        /// Gets the validators for the specified type.
        /// </summary>
        /// <param name="type">Type to validate.</param>
        /// <returns>Validators for the type.</returns>
        IEnumerable<IValidator> GetValidators(Type type);
    }
}