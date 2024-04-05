using Budget.Core.DataAccess;
using Budget.Core.UseCases.Transactions.Overview;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;

namespace Budget.Api;

public static class StartupExtensions
{
    public static IServiceCollection AddAllApiServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddDatabase(config)
            .AddEntraAuthentication(config.GetSection("AzureAd"))
            .AddUseCases()
            .AddSwagger(config.GetSection("Swagger"))
            .AddControllers();

        return services;
    }

    public static WebApplication UseBudgetApi(this WebApplication app, bool isDevelopment,
        IConfiguration config)
    {
        app.UseSwaggerWithAuth(isDevelopment, config.GetSection("Swagger"));
        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        return app;
    }

    internal static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<BudgetContext>(options =>
            options.UseSqlServer(config.GetConnectionString("BudgetContext"),
                b => b.MigrationsAssembly(typeof(BudgetContext).Assembly.FullName))
        );
        return services;
    }

    internal static IServiceCollection AddEntraAuthentication(this IServiceCollection services,
        IConfigurationSection sectionAzureAd)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(sectionAzureAd);
        return services;
    }

    internal static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        typeof(OverviewUseCase).Assembly.GetTypes()
            .Where(t => t.Name.EndsWith("UseCase"))
            .ToList()
            .ForEach(t => services.AddTransient(t));
        return services;
    }

    internal static IServiceCollection AddSwagger(this IServiceCollection services,
        IConfigurationSection configOfSwagger)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    Implicit = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(configOfSwagger.GetValue<string>("AuthorizeUrl") ?? "",
                            UriKind.Absolute),
                        Scopes = new Dictionary<string, string>
                        {
                            { configOfSwagger.GetValue<string>("Scopes") ?? "", "" },
                        }
                    }
                }
            });

            // This makes sure that Swagger UI passes a bearer token to each request instead of cookies
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                            { Type = ReferenceType.SecurityScheme, Id = JwtBearerDefaults.AuthenticationScheme }
                    },
                    new string[] { }
                }
            });
        });
        services.AddOpenApiDocument(config =>
        {
            config.SchemaSettings.DefaultReferenceTypeNullHandling =
                NJsonSchema.Generation.ReferenceTypeNullHandling.NotNull;
        });
        return services;
    }

    internal static IApplicationBuilder UseSwaggerWithAuth(this IApplicationBuilder app, bool isDevelopment,
        IConfigurationSection sectionOfSwagger)
    {
        if (isDevelopment)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.OAuthClientId(sectionOfSwagger.GetValue<string>("ClientId"));
                c.OAuthScopes(sectionOfSwagger.GetValue<string>("Scopes"));
            });
        }

        return app;
    }
}