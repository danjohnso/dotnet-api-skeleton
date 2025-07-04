﻿using System.Linq.Expressions;
using System.Reflection;
using Microsoft.OpenApi.Models;

namespace Skeleton.API.Core.Swashbuckle.FluentValidation.Core
{
    /// <summary>
    /// Extensions for <see cref="OpenApiSchema"/>.
    /// </summary>
    public static class OpenApiExtensions
    {
        /// <summary>
        /// Sets Nullable to false if MinLength > 0.
        /// </summary>
        internal static void SetNotNullableIfMinLengthGreaterThenZero(this OpenApiSchema schemaProperty)
        {
            if (schemaProperty.MinLength.HasValue && schemaProperty.MinLength > 0)
            {
                schemaProperty.Nullable = false;
            }
        }

        internal static void SetNewMax(this OpenApiSchema schemaProperty, Expression<Func<OpenApiSchema, int?>> prop, int? newValue)
        {
            if (newValue.HasValue)
            {
                int? current = prop.Compile()(schemaProperty);
                newValue = NewMaxValue(current, newValue.Value);
                schemaProperty.SetPropertyValue(prop, newValue);
            }
        }

        internal static void SetNewMax(this OpenApiSchema schemaProperty, Expression<Func<OpenApiSchema, decimal?>> prop, decimal? newValue)
        {
            if (newValue.HasValue)
            {
                decimal? current = prop.Compile()(schemaProperty);
                newValue = NewMaxValue(current, newValue.Value);
                schemaProperty.SetPropertyValue(prop, newValue);
            }
        }

        internal static void SetNewMin(this OpenApiSchema schemaProperty, Expression<Func<OpenApiSchema, int?>> prop, int? newValue, bool setNotNullableIfMinLengthGreaterThenZero = true)
        {
            if (newValue.HasValue)
            {
                int? current = prop.Compile()(schemaProperty);
                newValue = NewMinValue(current, newValue.Value);
                schemaProperty.SetPropertyValue(prop, newValue);
            }

            // SetNotNullableIfMinLengthGreaterThenZero should be optionated because FV allows nulls for MinLength validator
            if (setNotNullableIfMinLengthGreaterThenZero)
            {
                schemaProperty.SetNotNullableIfMinLengthGreaterThenZero();
            }
        }

        internal static void SetNewMin(this OpenApiSchema schemaProperty, Expression<Func<OpenApiSchema, decimal?>> prop, decimal? newValue, bool setNotNullableIfMinLengthGreaterThenZero = true)
        {
            if (newValue.HasValue)
            {
                decimal? current = prop.Compile()(schemaProperty);
                newValue = NewMinValue(current, newValue.Value);
                schemaProperty.SetPropertyValue(prop, newValue);
            }

            // SetNotNullableIfMinLengthGreaterThenZero should be optionated because FV allows nulls for MinLength validator
            if (setNotNullableIfMinLengthGreaterThenZero)
            {
                schemaProperty.SetNotNullableIfMinLengthGreaterThenZero();
            }
        }

        private static int NewMaxValue(int? current, int newValue) => current.HasValue ? Math.Min(current.Value, newValue) : newValue;

        private static decimal NewMaxValue(decimal? current, decimal newValue) => current.HasValue ? Math.Min(current.Value, newValue) : newValue;

        private static int NewMinValue(int? current, int newValue) => current.HasValue ? Math.Max(current.Value, newValue) : newValue;

        private static decimal NewMinValue(decimal? current, decimal newValue) => current.HasValue ? Math.Max(current.Value, newValue) : newValue;

        private static void SetPropertyValue<T, TValue>(this T target, Expression<Func<T, TValue>> propertyLambda, TValue value)
        {
            if (propertyLambda.Body is MemberExpression memberSelectorExpression)
            {
                PropertyInfo? property = memberSelectorExpression.Member as PropertyInfo;
                property?.SetValue(target, value, null);
            }
        }
    }
}