namespace Skeleton.API.v1.Requests
{
    public class FileUploadRequest
    {
        public required IFormFile File { get; set; }
        public required Guid ParentId { get; set; }
    }
}
