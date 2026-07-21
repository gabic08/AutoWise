namespace AutoWise.CommonUtilities.Messaging.Contracts.Media;

public record MediaAttachmentUploaded(
    Guid MediaAttachmentId,
    string ParentType,
    Guid ParentEntityId,
    string OriginalFileName,
    string ContentType,
    long SizeInBytes);
