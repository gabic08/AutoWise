namespace AutoWise.CommonUtilities.Repository.Abstractions;

public interface IPersistenceContext
{
    Task<int> SaveAsync();
}
