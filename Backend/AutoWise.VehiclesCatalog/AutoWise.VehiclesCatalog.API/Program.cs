var builder = WebApplication.CreateBuilder(args);

var assembly = typeof(Program).Assembly;

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

builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddSingleton<GetVehicleSpecificationsConfig>();


var app = builder.Build();

//app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapCarter();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();


app.Run();
