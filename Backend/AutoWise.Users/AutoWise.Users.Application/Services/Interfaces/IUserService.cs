namespace AutoWise.Users.Application.Services.Interfaces;

public interface IUserService
{
    Task<CreateOrSyncUserResponse> CreateOrSyncUserAsync(CreateOrSyncUserRequest request, CancellationToken ct = default);
}
