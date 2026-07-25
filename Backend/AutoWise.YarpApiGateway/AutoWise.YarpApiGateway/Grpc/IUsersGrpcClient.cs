using AutoWise.Users.Infrastructure.Grpc;

namespace AutoWise.YarpApiGateway.Grpc;

public interface IUsersGrpcClient
{
    Task<CreateOrSyncUserResponse> CreateOrSyncUserAsync(
        string externalId, string provider, string email, string displayName, CancellationToken ct = default);
}
