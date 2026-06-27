using AutoWise.VehiclesCatalog.API.Configurations;
using AutoWise.VehiclesCatalog.API.Grpc.Services;

var builder = WebApplication.CreateBuilder(args);
var assembly = typeof(Program).Assembly;

// Add services to the container.

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    config.AddOpenBehavior(typeof(ValidationBehaviour<,>));
});
builder.Services.AddValidatorsFromAssembly(assembly);


builder.Services.AddOpenApi();
builder.Services.AddCarter();

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.AddGrpc();

builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddSingleton<GetVehicleSpecificationsConfig>();

builder.Services.AddExceptionHandler<CustomExceptionHandler>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

var app = builder.Build();

app.MapCarter();

app.UseExceptionHandler(options => { });

app.MapGrpcService<VehicleSpecificationsService>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();


app.Run();
