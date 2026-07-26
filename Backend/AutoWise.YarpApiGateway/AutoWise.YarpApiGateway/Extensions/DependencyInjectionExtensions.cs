using AutoWise.Users.Infrastructure.Grpc;
using AutoWise.YarpApiGateway.Auth;
using AutoWise.YarpApiGateway.Grpc;
using Microsoft.AspNetCore.Authentication.JwtBearer;

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
                    .AllowAnyMethod();
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
