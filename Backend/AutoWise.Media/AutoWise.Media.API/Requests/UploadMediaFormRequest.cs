namespace AutoWise.Media.API.Requests;

public class UploadMediaFormRequest
{
    public IFormFile File { get; set; }
    public string ParentType { get; set; }
    public Guid ParentEntityId { get; set; }
}
