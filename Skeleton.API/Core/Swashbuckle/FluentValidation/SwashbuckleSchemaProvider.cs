﻿using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Skeleton.API.Core.Swashbuckle.FluentValidation
{
    /// <summary>
    /// SchemaProvider implementation for Swashbuckle.
    /// </summary>
    public class SwashbuckleSchemaProvider : ISchemaProvider<OpenApiSchema>
    {
        private readonly SchemaRepository _schemaRepository;
        private readonly ISchemaGenerator _schemaGenerator;
        private readonly Func<Type, string> _schemaIdSelector;

        /// <summary>
        /// Initializes a new instance of the <see cref="SwashbuckleSchemaProvider"/> class.
        /// </summary>
        /// <param name="schemaRepository">Swashbuckle schema repository.</param>
        /// <param name="schemaGenerator">Swashbuckle schema generator.</param>
        /// <param name="schemaIdSelector">Swashbuckle schemaId selector.</param>
        public SwashbuckleSchemaProvider(
            SchemaRepository schemaRepository,
            ISchemaGenerator schemaGenerator,
            Func<Type, string>? schemaIdSelector = null)
        {
            _schemaRepository = schemaRepository;
            _schemaGenerator = schemaGenerator;
            _schemaIdSelector = schemaIdSelector ?? DefaultSchemaIdSelector;
        }

        /// <inheritdoc />
        public OpenApiSchema GetSchemaForType(Type type)
        {
            var schemaId = _schemaIdSelector(type);

            if (!_schemaRepository.Schemas.TryGetValue(schemaId, out OpenApiSchema? schema))
            {
                schema = _schemaGenerator.GenerateSchema(type, _schemaRepository);
            }

            if ((schema.Properties == null || schema.Properties.Count == 0) &&
                _schemaRepository.Schemas.TryGetValue(schemaId, out OpenApiSchema? value))
            {
                schema = value;
            }

            return schema;
        }

        private string DefaultSchemaIdSelector(Type type)
        {
            return type.Name;
        }
    }
}