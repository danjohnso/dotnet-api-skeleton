using Skeleton.API.Core.Validation.File;

namespace Skeleton.API.Core.Validation.File
{
    public static class FileValidator
    {
        private static readonly List<FileFormatDescriptor> AllowedFormats = [new Png(), new Jpg(), new Pdf(), new Document()];

        public static FileFormatValidationResult IsValidFormat(IFormFile file)
        {
            string fileExtension = Path.GetExtension(file.FileName);
            FileFormatDescriptor? targetType = AllowedFormats.FirstOrDefault(x => x.IsIncludedExtension(fileExtension));
            if (targetType is null)
            {
                return new FileFormatValidationResult(false, FileFormatStatusType.NOT_SUPPORTED, $"{FileFormatStatusType.NOT_SUPPORTED}");
            }

            return targetType.Validate(file);
        }

        public static bool Exists(IFormFile file)
        {
            return file is not null && file.Length > 0;
        }

        public static bool IsValidSize(IFormFile file, long maxSizeInBytes)
        {
            return file.Length <= maxSizeInBytes;
        }
    }
}
