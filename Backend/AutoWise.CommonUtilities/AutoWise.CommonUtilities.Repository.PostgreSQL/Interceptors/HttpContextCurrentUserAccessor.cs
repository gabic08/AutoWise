using Microsoft.AspNetCore.Http;

namespace AutoWise.CommonUtilities.Persistence.PostgreSQL.Interceptors;

public class HttpContextCurrentUserAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentUserAccessor
{
    public Guid? UserId
    {
        get
        {
            var headerValue = httpContextAccessor.HttpContext?.Request.Headers["X-User-Id"].FirstOrDefault();
            return Guid.TryParse(headerValue, out var userId) ? userId : null;
        }
    }
}
