using Budget.Api.Server;
using Budget.Application.Providers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace Budget.Api;

public static class StartupExtensions
{
    public static void AddBudgetApi(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        var config = builder.Configuration;

        services.AddControllers();
        services.AddOpenApi(); // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        services.AddHttpContextAccessor();
        services.AddScoped<IUserProvider, UserProvider>();
        services.AddBudgetAuthentication(config, builder.Environment);
        services.AddSerilog((sp, lc) =>
        {
            lc.ReadFrom.Configuration(config)
                .ReadFrom.Services(sp)
                .Enrich.FromLogContext()
                .WriteTo.Console(
                    outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] ({SourceContext}) {Message:lj}{NewLine}{Exception}");
        });
        builder.Services.AddCors(options =>
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin();
            }));
    }

    private static void AddBudgetAuthentication(this IServiceCollection services, IConfiguration configuration,
        IHostEnvironment environment)
    {
        if (environment.IsDevelopment() && configuration.GetValue<bool>("Authentication:UseInDevelopment"))
        {
            services.AddAuthentication("FakeJwt")
                .AddScheme<FakeJwtOptions, FakeAuthHandler>("FakeJwt", null);
        }
        else
        {
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.Authority = configuration["Authentication:Authority"];
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidIssuer = configuration["Authentication:Issuer"],
                        ValidAudiences = configuration.GetSection("Authentication:Audiences").Get<string[]>(),
                    };
                });
        }
    }
}