namespace AutoWise.CommonUtilities.Repository.Abstractions;

public interface IAuditableDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
