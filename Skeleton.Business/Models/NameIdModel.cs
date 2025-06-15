namespace Skeleton.Business.Models
{
    /// <summary>
    /// Used for display type lists, use DropdownModel if you need to link with a dropdown/multiselect
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="Name"></param>
    public record NameIdModel(Guid Id, string Name);
}
