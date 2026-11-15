using AutoWise.Users.Infrastructure.Grpc;
using AutoWise.YarpApiGateway.Auth;
using AutoWise.YarpApiGateway.Grpc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace AutoWise.YarpApiGateway.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var activeAuthProvider = configuration.GetValue<AuthProvider>("Auth:ActiveProvider");
        var (authority, audience) = activeAuthProvider switch
        {
            AuthProvider.AzureAD => (
                configuration[$"{AzureAdAuthOptions.SectionName}:Authority"],
                configuration[$"{AzureAdAuthOptions.SectionName}:Audience"]),
            AuthProvider.Keycloak => (
                configuration[$"{KeycloakAuthOptions.SectionName}:Authority"],
                configuration[$"{KeycloakAuthOptions.SectionName}:Audience"]),
            _ => throw new InvalidOperationException($"Unsupported auth provider '{activeAuthProvider}'.")
        };

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.Audience = audience;
                options.MapInboundClaims = false;
            });

        services.AddAuthorization();

        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    // Lets the browser read the file name on media downloads.
                    .WithExposedHeaders("Content-Disposition");
            });
        });

        return services;
    }

    public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var permitLimit = configuration.GetValue("RateLimiting:PermitLimit", 100);
        var window = TimeSpan.FromSeconds(configuration.GetValue("RateLimiting:WindowSeconds", 10));

        var provider = configuration["Auth:ActiveProvider"];
        var externalIdClaim = configuration[$"Auth:{provider}:Claims:ExternalId"] ?? "sub";

        services.AddRateLimiter(rateLimiterOptions =>
        {
            rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // One window per signed-in user, falling back to the client IP, so users don't share a single quota.
            rateLimiterOptions.AddPolicy("fixed", httpContext =>
            {
                var partitionKey = httpContext.User.FindFirst(externalIdClaim)?.Value
                    ?? httpContext.Connection.RemoteIpAddress?.ToString()
                    ?? "anonymous";

                return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = permitLimit,
                    Window = window
                });
            });
        });

        return services;
    }

    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Connection string 'Redis' not found.");

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
        });

        return services;
    }

    public static IServiceCollection AddUsersGrpcClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddGrpcClient<UserProtoService.UserProtoServiceClient>(options =>
        {
            options.Address = new Uri(configuration.GetSection("GrpcSettings:UsersUrl").Value!);
        });
        services.AddScoped<IUsersGrpcClient, UsersGrpcClient>();

        return services;
    }
}
