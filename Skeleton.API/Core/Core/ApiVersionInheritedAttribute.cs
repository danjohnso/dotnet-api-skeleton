using Asp.Versioning;

namespace Skeleton.API.Core.Core
{
    /// <summary>
    /// This attribute allows the ApiVersion attribute to be inherited.  
    /// By design, the ApiVersion attribute cannot be inherited which means the attribute has to be 
    /// set on every controller.
    /// This attribute only needs to be added to the base controller for each version folder.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class ApiVersionInheritedAttribute : ApiVersionsBaseAttribute, IApiVersionProvider
    {
        private ApiVersionProviderOptions options;

        ApiVersionProviderOptions IApiVersionProvider.Options => options;

        /// <summary>
        /// Gets or sets a value indicating whether the specified set of API versions are deprecated
        /// </summary>
        /// <value>
        /// True if the specified set of API versions are deprecated; otherwise, false
        /// The default value is false.
        /// </value>
        public bool Deprecated
        {
            get
            {
                return (options & ApiVersionProviderOptions.Deprecated) == ApiVersionProviderOptions.Deprecated;
            }
            set
            {
                if (value)
                {
                    options |= ApiVersionProviderOptions.Deprecated;
                }
                else
                {
                    options &= ~ApiVersionProviderOptions.Deprecated;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the Microsoft.AspNetCore.Mvc.ApiVersionAttribute class
        /// </summary>
        /// <param name="version">The API version</param>
        protected ApiVersionInheritedAttribute(ApiVersion version)
            : base(version)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Microsoft.AspNetCore.Mvc.ApiVersionAttribute class
        /// </summary>
        /// <param name="version">The API version string</param>
        public ApiVersionInheritedAttribute(string version)
            : base(version)
        {
        }

        /// <summary>
        /// Returns a hash code for the current instance
        /// </summary>
        /// <returns>A hash code</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode() * 397 ^ Deprecated.GetHashCode();
        }
    }
}
