using FluentValidation;
using Microsoft.Extensions.Options;

namespace Skeleton.API.Core.Swashbuckle.FluentValidation.ValidatorRegistry
{
    /// <summary>
    /// <see cref="IValidatorRegistry"/> that works with registered validators.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ValidatorRegistry"/> class.
    /// </remarks>
    /// <param name="validators">Validators.</param>
    /// <param name="options">Generation options.</param>
    public class ValidatorRegistry(IEnumerable<IValidator> validators, IOptions<SchemaGenerationOptions>? options = null) : IValidatorRegistry
    {
        private readonly ISchemaGenerationOptions _options = options?.Value ?? new SchemaGenerationOptions();
        private readonly List<IValidator> _validators = [.. validators];

        /// <inheritdoc />
        public IValidator? GetValidator(Type type)
        {
            return GetValidators(type).FirstOrDefault();
        }

        /// <inheritdoc />
        public IEnumerable<IValidator> GetValidators(Type type)
        {
            return _validators.GetValidators(type, _options);
        }
    }
}