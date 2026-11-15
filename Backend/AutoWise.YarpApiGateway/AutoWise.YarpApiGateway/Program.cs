using AutoWise.YarpApiGateway.Extensions;
using AutoWise.YarpApiGateway.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddServiceDiscovery();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

builder.Services.AddAuthenticationServices(builder.Configuration);
builder.Services.AddRateLimiting(builder.Configuration);
builder.Services.AddCorsPolicy(builder.Configuration);
builder.Services.AddRedisCache(builder.Configuration);
builder.Services.AddUsersGrpcClient(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

// After authentication so the limiter can partition by the signed-in user.
app.UseRateLimiter();

app.UseMiddleware<UserSessionMiddleware>();

app.MapReverseProxy().RequireAuthorization();

app.Run();
