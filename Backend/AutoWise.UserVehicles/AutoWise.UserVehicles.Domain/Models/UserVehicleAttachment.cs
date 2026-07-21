using AutoWise.CommonUtilities.Models.BaseEntities;

namespace AutoWise.UserVehicles.Domain.Models;

public class UserVehicleAttachment : CreatedAuditBaseEntity
{
    public string ContentType { get; private set; }
    public Guid MediaAttachmentId { get; private set; }
    public string OriginalFileName { get; private set; }
    public long SizeInBytes { get; private set; }
    public UserVehicle UserVehicle { get; private set; }
    public Guid UserVehicleId { get; private set; }

    private UserVehicleAttachment() { }

    public static UserVehicleAttachment Create(
        Guid userVehicleId, Guid mediaAttachmentId, string originalFileName, string contentType, long sizeInBytes)
    {
        var attachment = new UserVehicleAttachment
        {
            UserVehicleId = userVehicleId,
            MediaAttachmentId = mediaAttachmentId
        };

        attachment.SetOriginalFileName(originalFileName);
        attachment.SetContentType(contentType);
        attachment.SetSizeInBytes(sizeInBytes);

        return attachment;
    }

    private void SetOriginalFileName(string originalFileName)
    {
        if (string.IsNullOrWhiteSpace(originalFileName))
        {
            throw new ArgumentException("Original file name is required.", nameof(originalFileName));
        }
        OriginalFileName = originalFileName.Trim();
    }

    private void SetContentType(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new ArgumentException("Content type is required.", nameof(contentType));
        }
        ContentType = contentType.Trim();
    }

    private void SetSizeInBytes(long sizeInBytes)
    {
        if (sizeInBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sizeInBytes), "Size must be greater than zero.");
        }
        SizeInBytes = sizeInBytes;
    }
}
