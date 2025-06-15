namespace Skeleton.Business.Models
{
    /// <summary>
    /// Should be used when creating dropdown lists or when making fields of selected values in a dropdown list
    /// </summary>
    /// <param name="Display"></param>
    /// <param name="Value"></param>
    public record DropdownModel(string Display, string Value);
}
