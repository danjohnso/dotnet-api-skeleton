using Skeleton.API.Streaming;
using Skeleton.API.Core.Swagger;
using Skeleton.API.Core.Swashbuckle.FluentValidation.AspNetCore;

namespace Skeleton.API.Core
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers Swagger with the service provider
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="configurationSectionName"></param>
        /// <returns></returns>
        public static void AddSwagger(this IServiceCollection services, IConfiguration configuration, string configurationSectionName = "Swagger")
        {
            services.Configure<SwaggerConfigurationOptions>(configuration.GetSection(configurationSectionName));
            services.ConfigureOptions<ConfigureSwaggerGenOptions>();
            services.AddSwaggerGen(_ =>
            {
                _.OperationFilter<StreamedFileOperation>();
            });
            services.AddFluentValidationRulesToSwagger();
        }
    }
}
