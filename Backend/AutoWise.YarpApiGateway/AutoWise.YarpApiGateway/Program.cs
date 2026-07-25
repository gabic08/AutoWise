using AutoWise.YarpApiGateway.Extensions;
using AutoWise.YarpApiGateway.Middleware;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddRateLimiter(rateLimiterOptions =>
{
    rateLimiterOptions.AddFixedWindowLimiter("fixed", options =>
    {
        options.PermitLimit = 5;
        options.Window = TimeSpan.FromSeconds(10);
    });
});

builder.Services.AddAuthenticationServices(builder.Configuration);
builder.Services.AddRedisCache(builder.Configuration);
builder.Services.AddUsersGrpcClient(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<UserSessionMiddleware>();

app.MapReverseProxy().RequireAuthorization();

app.Run();
