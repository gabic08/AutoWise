namespace AutoWise.Media.Application.Config;

public class MediaUploadOptions
{
    public const string SectionName = "AllowedMimeTypes";

    public List<string> AllowedMimeTypes { get; set; } = [];
}
