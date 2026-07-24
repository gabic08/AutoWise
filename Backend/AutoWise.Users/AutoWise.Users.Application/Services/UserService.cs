namespace AutoWise.Users.Application.Services;

public class UserService(IUsersDbContext dbContext) : IUserService
{
    public async Task<CreateOrSyncUserResponse> CreateOrSyncUserAsync(CreateOrSyncUserRequest request, CancellationToken ct = default)
    {
        var existingUser = await dbContext.Users.FirstOrDefaultAsync
            (u => u.Provider == request.Provider && u.ExternalId == request.ExternalId, cancellationToken: ct);

        if (existingUser is not null)
        {
            if (existingUser.ProfileNeedsUpdate(request.Email, request.DisplayName))
            {
                existingUser.UpdateProfile(request.Email, request.DisplayName);
                await dbContext.SaveChangesAsync(ct);
            }

            return NewCreateOrSyncUserResponse(existingUser);
        }

        var newUser = User.Create(request.DisplayName, request.Email, request.ExternalId, request.Provider);
        await dbContext.Users.AddAsync(newUser, ct);
        await dbContext.SaveChangesAsync(ct);

        return NewCreateOrSyncUserResponse(newUser);
    }

    private static CreateOrSyncUserResponse NewCreateOrSyncUserResponse(User user)
    {
        return new CreateOrSyncUserResponse(user.Id, user.ExternalId, user.Provider, user.Email, user.DisplayName);
    }
}
