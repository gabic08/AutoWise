using AutoWise.CommonUtilities.Models.BaseEntities;

namespace AutoWise.Media.Domain.Models;

public class MediaAttachment : CreatedAuditBaseEntity
{
    public Guid MediaFileId { get; private set; }
    public MediaFile MediaFile { get; private set; }
    public string OriginalFileName { get; private set; }
    public Guid ParentEntityId { get; private set; }
    public string ParentType { get; private set; }

    private MediaAttachment() { }

    public static MediaAttachment Create(Guid mediaFileId, string parentType, Guid parentEntityId, string originalFileName)
    {
        var attachment = new MediaAttachment
        {
            MediaFileId = mediaFileId,
        };

        attachment.SetParentType(parentType);
        attachment.SetParentEntityId(parentEntityId);
        attachment.SetOriginalFileName(originalFileName);

        return attachment;
    }

    private void SetParentType(string parentType)
    {
        if (string.IsNullOrWhiteSpace(parentType))
        {
            throw new ArgumentException("Parent type is required.", nameof(parentType));
        }
        ParentType = parentType.Trim();
    }

    private void SetParentEntityId(Guid parentEntityId)
    {
        if (parentEntityId == Guid.Empty)
        {
            throw new ArgumentException("Parent entity id is required.", nameof(parentEntityId));
        }
        ParentEntityId = parentEntityId;
    }

    private void SetOriginalFileName(string originalFileName)
    {
        if (string.IsNullOrWhiteSpace(originalFileName))
        {
            throw new ArgumentException("Original file name is required.", nameof(originalFileName));
        }
        OriginalFileName = originalFileName.Trim();
    }
}
