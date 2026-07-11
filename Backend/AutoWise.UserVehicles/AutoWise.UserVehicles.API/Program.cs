using AutoWise.UserVehicles.API.Extensions;
using AutoWise.UserVehicles.Application.Extensions;
using AutoWise.UserVehicles.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration)
    .AddApiServices();







builder.Services.AddControllers();
builder.Services.AddOpenApi();
//builder.AddDbContext();

//builder.Services.AddScoped<IUserVehicleRepository, UserVehicleRepository>();

//builder.Services.AddScoped<IUserVehicleService, UserVehicleService>();

//builder.Services.AddGrpcClient<VehicleSpecificationsProtoService.VehicleSpecificationsProtoServiceClient>(options =>
//{
//    options.Address = new Uri(builder.Configuration.GetSection("GrpcSettings:VehiclesCatalogUrl").Value);
//});

//if (builder.Environment.IsDevelopment())
//{
//    builder.Configuration.AddUserSecrets<Program>();
//}


//builder.Services.AddGrpc();
//builder.Services.AddExceptionHandler<CustomExceptionHandler>();

//builder.Services.AddStackExchangeRedisCache(options =>
//{
//    options.Configuration = builder.Configuration.GetConnectionString("Redis");
//});


var app = builder.Build();



// Configure the HTTP request pipeline.
app.UseApiServices();

await app.ApplyMigrationsAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
