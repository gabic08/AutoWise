namespace AutoWise.Users.Tests.TestDoubles;

public class InMemoryUsersDbContext(DbContextOptions<InMemoryUsersDbContext> options)
    : DbContext(options), IUsersDbContext
{
    public DbSet<User> Users => Set<User>();

    public static InMemoryUsersDbContext Create()
    {
        var options = new DbContextOptionsBuilder<InMemoryUsersDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new InMemoryUsersDbContext(options);
    }
}
