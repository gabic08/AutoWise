using AutoWise.CommonUtilities.ExtensionMethods;
using AutoWise.YarpApiGateway.Grpc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace AutoWise.YarpApiGateway.Middleware;

public class UserSessionMiddleware(RequestDelegate next)
{
    private const string UserIdHeaderName = "X-User-Id";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    public async Task InvokeAsync(HttpContext context, IUsersGrpcClient usersGrpcClient, IDistributedCache cache, IConfiguration configuration)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var provider = configuration["Auth:ActiveProvider"];

            var externalIdClaim = configuration[$"Auth:{provider}:Claims:ExternalId"] ?? "sub";
            var emailClaim = configuration[$"Auth:{provider}:Claims:Email"] ?? "email";
            var displayNameClaim = configuration[$"Auth:{provider}:Claims:DisplayName"] ?? "name";

            var externalId = context.User.FindFirst(externalIdClaim)?.Value;
            var email = context.User.FindFirst(emailClaim)?.Value ?? string.Empty;
            var displayName = context.User.FindFirst(displayNameClaim)?.Value ?? string.Empty;

            if (externalId.NotNullOrEmpty() && provider.NotNullOrEmpty())
            {
                var cacheKey = $"user:{provider}:{externalId}";
                var cachedSession = await GetCachedSessionAsync(cache, cacheKey, context.RequestAborted);

                if (cachedSession is null || cachedSession.Email != email || cachedSession.DisplayName != displayName)
                {
                    var response = await usersGrpcClient.CreateOrSyncUserAsync(
                        externalId, provider, email, displayName, context.RequestAborted);

                    cachedSession = new CachedUserSession(response.Id, response.Email, response.DisplayName);

                    await cache.SetStringAsync(
                        cacheKey,
                        JsonSerializer.Serialize(cachedSession),
                        new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheDuration },
                        context.RequestAborted);
                }

                context.Request.Headers[UserIdHeaderName] = cachedSession.UserId;
            }
        }
        else
        {
            context.Request.Headers.Remove(UserIdHeaderName);
        }

        await next(context);
    }

    private static async Task<CachedUserSession> GetCachedSessionAsync(IDistributedCache cache, string cacheKey, CancellationToken ct)
    {
        var cachedValue = await cache.GetStringAsync(cacheKey, ct);
        return cachedValue is null ? null : JsonSerializer.Deserialize<CachedUserSession>(cachedValue);
    }
}

public record CachedUserSession(string UserId, string Email, string DisplayName);
