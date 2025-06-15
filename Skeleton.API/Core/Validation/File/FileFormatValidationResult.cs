namespace Skeleton.API.Core.Validation.File
{
    public record FileFormatValidationResult(bool IsAcceptable, FileFormatStatusType Status, string Message);
}
