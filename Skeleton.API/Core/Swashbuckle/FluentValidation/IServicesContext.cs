namespace Skeleton.API.Core.Swashbuckle.FluentValidation
{
    /// <summary>
    /// Services that can be injected with DI.
    /// </summary>
    public interface IServicesContext
    {
        /// <summary>
        /// Gets optional <see cref="INameResolver"/>.
        /// </summary>
        INameResolver? NameResolver { get; }
    }

    /// <summary>
    /// Services that can be injected with DI.
    /// </summary>
    public class ServicesContext(INameResolver? nameResolver = null) : IServicesContext
    {
        /// <inheritdoc />
        public INameResolver? NameResolver { get; } = nameResolver;
    }
}