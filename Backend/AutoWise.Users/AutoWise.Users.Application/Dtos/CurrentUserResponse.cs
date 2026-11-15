namespace AutoWise.Users.Application.Dtos;

public record CurrentUserResponse(Guid Id, string Email, string DisplayName);
