namespace Skeleton.API.v1.Requests
{
    public class ThingCreateRequest
    {
        public string Name { get; set; } = "";
        public string? ModelNumber { get; set; }
        public Guid? ParentId { get; set; }
    }
}
