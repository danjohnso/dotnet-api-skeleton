namespace Skeleton.API.Core.Swashbuckle.FluentValidation
{
    /// <summary>
    /// Schema provider.
    /// </summary>
    /// <typeparam name="TSchema">Schema type.</typeparam>
    public interface ISchemaProvider<TSchema>
    {
        /// <summary>
        /// Gets or creates schema for type.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <returns>Schema.</returns>
        TSchema GetSchemaForType(Type type);
    }
}