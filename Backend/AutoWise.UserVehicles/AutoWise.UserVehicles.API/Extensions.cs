namespace AutoWise.UserVehicles.API;

public static class Extensions
{
    public static WebApplicationBuilder AddDbContext(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddDbContext<UserVehiclesDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")
            ?? throw new InvalidOperationException("Connection string 'PostgreSQL' not found.")));

        builder.Services.AddScoped<IAuditableDbContext>(sp => sp.GetRequiredService<UserVehiclesDbContext>());

        return builder;
    }

    public static IApplicationBuilder ApplyMigrations(this IApplicationBuilder app)
    {
        var serviceScope = app.ApplicationServices.CreateScope();
        var context = serviceScope.ServiceProvider.GetRequiredService<UserVehiclesDbContext>();
        context?.Database.Migrate();

        return app;
    }
}
