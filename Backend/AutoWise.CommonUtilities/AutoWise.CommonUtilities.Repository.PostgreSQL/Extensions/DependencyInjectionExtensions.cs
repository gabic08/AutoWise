using AutoWise.CommonUtilities.Persistence.PostgreSQL.Interceptors;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AutoWise.CommonUtilities.Persistence.PostgreSQL.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddAuditableEntityInterceptor(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserAccessor, HttpContextCurrentUserAccessor>();
        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();

        return services;
    }
}
