using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Skeleton.API.Core.Swashbuckle.FluentValidation
{
    /// <summary>
    /// Creates service from service provider with desired lifestyle.
    /// </summary>
    public class FluentValidationRulesScopeAdapter : ISchemaFilter
    {
        private readonly FluentValidationRules _fluentValidationRules;

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentValidationRulesScopeAdapter"/> class.
        /// </summary>
        /// <param name="serviceProvider"><see cref="IServiceProvider"/>.</param>
        /// <param name="serviceLifetime"><see cref="ServiceLifetime"/> to use.</param>
        public FluentValidationRulesScopeAdapter(IServiceProvider serviceProvider, ServiceLifetime serviceLifetime)
        {
            // Hack with the scope mismatch.
            if (serviceLifetime == ServiceLifetime.Scoped || serviceLifetime == ServiceLifetime.Transient)
            {
                serviceProvider = serviceProvider.CreateScope().ServiceProvider;
            }

#pragma warning disable CS8601 // Possible null reference assignment.
            _fluentValidationRules = serviceProvider.GetService<FluentValidationRules>();
#pragma warning restore CS8601 // Possible null reference assignment.
            if (_fluentValidationRules is null)
            {
                ILogger? logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(typeof(FluentValidationRulesScopeAdapter));
                logger?.LogWarning($"{nameof(FluentValidationRules)} should be registered in services. Hint: Use registration method '{nameof(AspNetCore.ServiceCollectionExtensions.AddFluentValidationRulesToSwagger)}'");
            }

            // Last chance to create filter
            _fluentValidationRules ??= ActivatorUtilities.CreateInstance<FluentValidationRules>(serviceProvider);
        }

        /// <inheritdoc />
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            _fluentValidationRules.Apply(schema, context);
        }
    }

    /// <summary>
    /// Creates service from service provider with desired lifestyle.
    /// </summary>
    public class FluentValidationOperationFilterScopeAdapter : IOperationFilter
    {
        private readonly FluentValidationOperationFilter _fluentValidationRules;

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentValidationOperationFilterScopeAdapter"/> class.
        /// </summary>
        /// <param name="serviceProvider"><see cref="IServiceProvider"/>.</param>
        /// <param name="serviceLifetime"><see cref="ServiceLifetime"/> to use.</param>
        public FluentValidationOperationFilterScopeAdapter(IServiceProvider serviceProvider, ServiceLifetime serviceLifetime)
        {
            // Hack with the scope mismatch.
            if (serviceLifetime == ServiceLifetime.Scoped || serviceLifetime == ServiceLifetime.Transient)
            {
                serviceProvider = serviceProvider.CreateScope().ServiceProvider;
            }

#pragma warning disable CS8601 // Possible null reference assignment.
            _fluentValidationRules = serviceProvider.GetService<FluentValidationOperationFilter>();
#pragma warning restore CS8601 // Possible null reference assignment.
            if (_fluentValidationRules is null)
            {
                ILogger? logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(typeof(FluentValidationRulesScopeAdapter));
                logger?.LogWarning($"{nameof(FluentValidationOperationFilter)} should be registered in services. Hint: Use registration method '{nameof(AspNetCore.ServiceCollectionExtensions.AddFluentValidationRulesToSwagger)}'");
            }

            // Last chance to create filter
            _fluentValidationRules ??= ActivatorUtilities.CreateInstance<FluentValidationOperationFilter>(serviceProvider);
        }

        /// <inheritdoc />
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            _fluentValidationRules.Apply(operation, context);
        }
    }

    /// <summary>
    /// Creates service from service provider with desired lifestyle.
    /// </summary>
    public class DocumentFilterScopeAdapter<TDocumentFilter> : IDocumentFilter
        where TDocumentFilter : IDocumentFilter
    {
        private readonly IDocumentFilter _documentFilter;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentFilterScopeAdapter{TDocumentFilter}"/> class.
        /// </summary>
        /// <param name="serviceProvider"><see cref="IServiceProvider"/>.</param>
        /// <param name="serviceLifetime"><see cref="ServiceLifetime"/> to use.</param>
        public DocumentFilterScopeAdapter(IServiceProvider serviceProvider, ServiceLifetime serviceLifetime)
        {
            // Hack with the scope mismatch.
            if (serviceLifetime == ServiceLifetime.Scoped || serviceLifetime == ServiceLifetime.Transient)
                serviceProvider = serviceProvider.CreateScope().ServiceProvider;

#pragma warning disable CS8601 // Possible null reference assignment.
            _documentFilter = serviceProvider.GetService<TDocumentFilter>();
#pragma warning restore CS8601 // Possible null reference assignment.
            if (_documentFilter is null)
            {
                ILogger? logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
                logger?.LogWarning($"{nameof(TDocumentFilter)} should be registered in services. Hint: Use registration method '{nameof(AspNetCore.ServiceCollectionExtensions.AddFluentValidationRulesToSwagger)}'");
            }

            // Last chance to create filter
            _documentFilter ??= ActivatorUtilities.CreateInstance<TDocumentFilter>(serviceProvider);
        }

        /// <inheritdoc />
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            _documentFilter.Apply(swaggerDoc, context);
        }
    }
}