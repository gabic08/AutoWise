namespace AutoWise.CommonUtilities.Persistence.Abstractions;

public interface IBaseDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
