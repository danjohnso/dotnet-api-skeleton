using FluentValidation;

namespace Skeleton.API.Core.Swashbuckle.FluentValidation.ValidatorRegistry
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers all registered validators as base <see cref="IValidator"/>.
        /// </summary>
        /// <param name="services">The source services.</param>
        /// <returns>Modified services.</returns>
        public static IServiceCollection RegisterValidatorsAsIValidator(this IServiceCollection services)
        {
            // Register all validators as IValidator?
            List<ServiceDescriptor> serviceDescriptors = [.. services.Where(descriptor => descriptor.ServiceType.GetInterfaces().Contains(typeof(IValidator)))];
            serviceDescriptors.ForEach(descriptor => services.Add(ServiceDescriptor.Describe(typeof(IValidator), descriptor.ImplementationType!, descriptor.Lifetime)));
            return services;
        }
    }
}