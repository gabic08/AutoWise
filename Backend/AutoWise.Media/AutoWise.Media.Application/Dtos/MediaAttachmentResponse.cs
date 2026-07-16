namespace AutoWise.Media.Application.Dtos;

public record MediaAttachmentResponse(
    Guid Id,
    string OriginalFileName,
    string ContentType,
    long SizeInBytes,
    string ParentType,
    Guid ParentEntityId);