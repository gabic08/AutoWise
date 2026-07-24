namespace AutoWise.Users.Application.Data;

public interface IUsersDbContext : IBaseDbContext
{
    DbSet<User> Users { get; }
}
