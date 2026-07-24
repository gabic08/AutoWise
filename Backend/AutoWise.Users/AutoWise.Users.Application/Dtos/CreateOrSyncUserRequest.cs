namespace AutoWise.Users.Application.Dtos;

public record CreateOrSyncUserRequest(string ExternalId, string Provider, string Email, string DisplayName);
