namespace Skeleton.Business.Models
{
    public class ThingModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public Guid? ParentId { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime Modified { get; set; }
        public DateTime Created { get; set; }
        public uint RowVersion { get; set; }
    }
}
