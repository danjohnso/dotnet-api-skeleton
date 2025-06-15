namespace Skeleton.API.Core.Swashbuckle.FluentValidation.Core
{
    /// <summary>
    /// Represents some matching condition.
    /// </summary>
    /// <typeparam name="T">Type for matching.</typeparam>
    public interface ICondition<in T>
    {
        /// <summary>
        /// Determine whether the value matches condition.
        /// </summary>
        /// <param name="value">The value to check against the condition.</param>
        /// <returns>true if the condition matches.</returns>
        bool Matches(T value);
    }
}