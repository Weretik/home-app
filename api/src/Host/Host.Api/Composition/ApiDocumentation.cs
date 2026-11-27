using System.Text.Json.Serialization;
using Microsoft.AspNetCore.OpenApi;

namespace Host.Api.Composition;

internal static class ApiDocumentation
{
    /// <summary>
    /// The OpenAPI document is generated from the endpoints and is the contract the frontend generates its client
    /// from. Numbers are strict JSON numbers and enums are strings, so the generated types are exact.
    /// </summary>
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        return services.AddOpenApi(options =>
        {
            options.CreateSchemaReferenceId = SchemaName;
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info.Title = "Home API";
                document.Info.Version = "v1";
                return Task.CompletedTask;
            });
        });
    }

    public static void MapApiDocumentation(this WebApplication app)
    {
        // The contract is public anyway (it is committed to the repository); the data behind it is not.
        app.MapOpenApi().AllowAnonymous();
        if (app.Environment.IsDevelopment())
            app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Home API"));
    }

    /// <summary>An endpoint's nested <c>Request</c> is named after the endpoint: <c>CreateProductRequest</c>.</summary>
    private static string? SchemaName(System.Text.Json.Serialization.Metadata.JsonTypeInfo info) =>
        info.Type.IsNested && info.Type.DeclaringType!.Name.EndsWith("Endpoint", StringComparison.Ordinal)
            ? info.Type.DeclaringType.Name[..^"Endpoint".Length] + info.Type.Name
            : OpenApiOptions.CreateDefaultSchemaReferenceId(info);
}
