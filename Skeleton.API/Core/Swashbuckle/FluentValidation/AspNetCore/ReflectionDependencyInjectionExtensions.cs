﻿using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Skeleton.API.Core.Swashbuckle.FluentValidation.AspNetCore
{
    /// <summary>
    /// DependencyInjection through Reflection.
    /// </summary>
    internal static class ReflectionDependencyInjectionExtensions
    {
        private static Type? GetByFullName(string typeName)
        {
            Type? type = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .Where(assembly => assembly.FullName?.Contains("Microsoft") ?? false)
                .SelectMany(GetLoadableTypes)
                .FirstOrDefault(type => type.FullName == typeName);

            return type;
        }

        /// <summary>
        /// Gets loadable Types from an Assembly, not throwing when some Types can't be loaded.
        /// </summary>
        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
                return e.Types.Where(t => t is not null);
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
            }
        }

        /// <summary>
        /// Calls through reflection: <c>services.Configure&lt;JsonOptions&gt;(options =&gt; configureJson(options));</c>.
        /// Can be used from netstandard.
        /// </summary>
        /// <param name="services">Services.</param>
        /// <param name="configureJson">Action to configure <see cref="JsonSerializerOptions"/> in JsonOptions.</param>
        public static void ConfigureJsonOptionsForAspNetCore(this IServiceCollection services, Action<JsonSerializerOptions> configureJson)
        {
            Action<object> configureJsonOptionsUntyped = options =>
            {
                PropertyInfo? propertyInfo = options.GetType().GetProperty("JsonSerializerOptions");

                if (propertyInfo?.GetValue(options) is JsonSerializerOptions jsonSerializerOptions)
                {
                    configureJson(jsonSerializerOptions);
                }
            };

            Type? jsonOptionsType = GetByFullName("Microsoft.AspNetCore.Mvc.JsonOptions");
            if (jsonOptionsType != null)
            {
                Type? extensionsType = GetByFullName("Microsoft.Extensions.DependencyInjection.OptionsServiceCollectionExtensions");

                MethodInfo? configureMethodGeneric = extensionsType
                    ?.GetTypeInfo()
                    .DeclaredMethods
                    .FirstOrDefault(info => info.Name == "Configure" && info.GetParameters().Length == 2);

                MethodInfo? configureMethod = configureMethodGeneric?.MakeGenericMethod(jsonOptionsType);

                // services.Configure<JsonOptions>(options => configureJson(options));
                configureMethod?.Invoke(services, [services, configureJsonOptionsUntyped]);
            }
        }

        /// <summary>
        /// Gets <see cref="JsonSerializerOptions"/> from JsonOptions registered in AspNetCore.
        /// Uses reflection to call code:
        /// <code>serviceProvider.GetService&lt;IOptions&lt;JsonOptions&gt;&gt;()?.Value?.JsonSerializerOptions;</code>
        /// </summary>
        /// <param name="serviceProvider">Source service provider.</param>
        /// <returns>Optional <see cref="JsonSerializerOptions"/>.</returns>
        public static JsonSerializerOptions? GetJsonSerializerOptions(this IServiceProvider serviceProvider)
        {
            JsonSerializerOptions? jsonSerializerOptions = null;

            Type? jsonOptionsType = GetByFullName("Microsoft.AspNetCore.Mvc.JsonOptions");
            if (jsonOptionsType != null)
            {
                // IOptions<JsonOptions>
                Type jsonOptionsInterfaceType = typeof(IOptions<>).MakeGenericType(jsonOptionsType);
                object? jsonOptionsOption = serviceProvider.GetService(jsonOptionsInterfaceType);

                if (jsonOptionsOption != null)
                {
                    PropertyInfo? valueProperty = jsonOptionsInterfaceType.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public);
                    PropertyInfo? jsonSerializerOptionsProperty = jsonOptionsType.GetProperty("JsonSerializerOptions", BindingFlags.Instance | BindingFlags.Public);

                    if (valueProperty != null && jsonSerializerOptionsProperty != null)
                    {
                        // JsonOptions
                        var jsonOptions = valueProperty.GetValue(jsonOptionsOption);

                        // JsonSerializerOptions
                        if (jsonOptions != null)
                        {
                            jsonSerializerOptions = jsonSerializerOptionsProperty.GetValue(jsonOptions) as JsonSerializerOptions;
                        }
                    }
                }
            }

            return jsonSerializerOptions;
        }

        public static JsonSerializerOptions GetJsonSerializerOptionsOrDefault(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetJsonSerializerOptions() ?? new JsonSerializerOptions();
        }
    }
}