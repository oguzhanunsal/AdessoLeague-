using AdessoLeague.Api.Contracts.Requests;
using AdessoLeague.Application.Contracts;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AdessoLeague.Api.Swagger;

public sealed class DrawExampleSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(schema);
        ArgumentNullException.ThrowIfNull(context);

        if (context.Type == typeof(CreateDrawRequest))
        {
            schema.Example = new OpenApiObject
            {
                ["groupCount"] = new OpenApiInteger(8),
                ["drawnBy"] = new OpenApiObject
                {
                    ["firstName"] = new OpenApiString("Oğuzhan"),
                    ["lastName"] = new OpenApiString("Ünsal"),
                },
            };
        }

        if (context.Type == typeof(GroupResponse))
        {
            schema.Example = new OpenApiObject
            {
                ["groupName"] = new OpenApiString("A"),
                ["teams"] = new OpenApiArray
                {
                    new OpenApiObject { ["name"] = new OpenApiString("Adesso İstanbul") },
                    new OpenApiObject { ["name"] = new OpenApiString("Adesso Berlin") },
                    new OpenApiObject { ["name"] = new OpenApiString("Adesso Paris") },
                    new OpenApiObject { ["name"] = new OpenApiString("Adesso Roma") },
                },
            };
        }
    }
}
