using AutoWise.Users.Infrastructure.Grpc;

namespace AutoWise.YarpApiGateway.Grpc;

public class UsersGrpcClient(UserProtoService.UserProtoServiceClient client) : IUsersGrpcClient
{
    public async Task<CreateOrSyncUserResponse> CreateOrSyncUserAsync(string externalId, string provider, string email, string displayName, CancellationToken ct = default)
    {
        var request = new CreateOrSyncUserRequest
        {
            ExternalId = externalId,
            Provider = provider,
            Email = email,
            DisplayName = displayName
        };

        return await client.CreateOrSyncUserAsync(request, cancellationToken: ct);
    }
}
