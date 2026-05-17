using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace BuildingBlocks.OpenApi;

public static class EshopOpenApiExtensions
{
    private const string BearerSecuritySchemeName = "BearerAuth";

    public static IServiceCollection AddEshopOpenApi(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes[BearerSecuritySchemeName] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Enter only the JWT access token. Scalar will send it as Authorization: Bearer {token}."
                };

                document.Security ??= [];
                document.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference(BearerSecuritySchemeName, document)] = []
                });

                return Task.CompletedTask;
            });
        });

        return services;
    }

    public static WebApplication UseEshopOpenApi(this WebApplication app, string title)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi()
                .AllowAnonymous();
            app.MapGet("/swagger", () => Results.Redirect("/scalar"))
                .AllowAnonymous();
            app.MapGet("/swagger/index.html", () => Results.Redirect("/scalar"))
                .AllowAnonymous();
            app.MapScalarApiReference(options => options
                    .WithTitle(title)
                    .AddPreferredSecuritySchemes(BearerSecuritySchemeName)
                    .AddHttpAuthentication(BearerSecuritySchemeName, _ => { })
                    .EnablePersistentAuthentication())
                .AllowAnonymous();
        }

        return app;
    }
}
