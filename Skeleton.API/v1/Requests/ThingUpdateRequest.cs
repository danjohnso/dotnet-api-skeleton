using Skeleton.Business.Models;

namespace Skeleton.API.v1.Requests
{
    public class ThingUpdateRequest
    {
        public string Name { get; set; } = "";
        public Guid? ParentId { get; set; }
        public uint? RowVersion { get; set; }
    }
}
