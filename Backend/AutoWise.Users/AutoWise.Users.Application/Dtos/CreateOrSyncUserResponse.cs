namespace AutoWise.Users.Application.Dtos;

public record CreateOrSyncUserResponse(Guid Id, string ExternalId, string Provider, string Email, string DisplayName);

