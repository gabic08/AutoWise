namespace AutoWise.CommonUtilities.Persistence.PostgreSQL.Interceptors;

public interface ICurrentUserAccessor
{
    Guid? UserId { get; }
}
