namespace Skeleton.API.Core.Swashbuckle.FluentValidation
{
    /// <summary>
    /// Provides rules for schema generation.
    /// </summary>
    /// <typeparam name="TSchema">Schema implementation type.</typeparam>
    public interface IFluentValidationRuleProvider<in TSchema>
    {
        /// <summary>
        /// Gets rules for schema generation.
        /// </summary>
        /// <returns>Enumeration of rules.</returns>
        IEnumerable<IFluentValidationRule<TSchema>> GetRules();
    }
}