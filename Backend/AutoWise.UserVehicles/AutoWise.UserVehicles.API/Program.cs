using AutoWise.UserVehicles.API;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.AddDbContext();

builder.Services.AddScoped<IPersistenceContext, PersistenceContext<UserVehiclesDbContext>>((provider) =>
{
    var userVehiclesPersistenceContext = new PersistenceContext<UserVehiclesDbContext>(provider);
    return userVehiclesPersistenceContext;
});

builder.Services.AddScoped<IUserVehicleRepository, UserVehicleRepository>();

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.AddGrpc();
builder.Services.AddExceptionHandler<CustomExceptionHandler>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.ApplyMigrations();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
