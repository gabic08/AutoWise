namespace AutoWise.Users.Infrastructure.Grpc.Services;

public class UserGrpcService(IUserService userService) : UserProtoService.UserProtoServiceBase
{
    public override async Task<CreateOrSyncUserResponse> CreateOrSyncUser(CreateOrSyncUserRequest request, ServerCallContext context)
    {
        var applicationLayerRequest = new Application.Dtos.CreateOrSyncUserRequest(
            request.ExternalId, request.Provider, request.Email, request.DisplayName);

        var result = await userService.CreateOrSyncUserAsync(applicationLayerRequest, context.CancellationToken);

        return new CreateOrSyncUserResponse
        {
            Id = result.Id.ToString(),
            ExternalId = result.ExternalId,
            Provider = result.Provider,
            Email = result.Email,
            DisplayName = result.DisplayName
        };
    }
}
