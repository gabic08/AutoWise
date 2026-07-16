namespace AutoWise.Media.Application.Dtos;

public record UploadMediaRequest(
    Stream Content,
    string ContentType,
    string FileName,
    string ParentType,
    Guid ParentEntityId);
