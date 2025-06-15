using FluentValidation;
using Microsoft.Extensions.Options;
using Skeleton.API.Core.Swashbuckle.FluentValidation;

namespace Skeleton.API.Core.Swashbuckle.FluentValidation.ValidatorRegistry
{
    /// <summary>
    /// Validator registry that gets validators from <see cref="IServiceProvider"/>.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ServiceProviderValidatorRegistry"/> class.
    /// </remarks>
    /// <param name="serviceProvider">The source service provider.</param>
    /// <param name="options">Schema generation options.</param>
    public class ServiceProviderValidatorRegistry(IServiceProvider serviceProvider, IOptions<SchemaGenerationOptions>? options = null) : IValidatorRegistry
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        private readonly ISchemaGenerationOptions _options = options?.Value ?? new SchemaGenerationOptions();

        /// <inheritdoc />
        public IValidator? GetValidator(Type type) => _serviceProvider.GetValidator(type);

        /// <inheritdoc />
        public IEnumerable<IValidator> GetValidators(Type type) => _serviceProvider.GetValidators(type, _options);
    }
}