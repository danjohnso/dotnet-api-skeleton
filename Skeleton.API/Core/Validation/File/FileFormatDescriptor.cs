namespace Skeleton.API.Core.Validation.File
{
    /// <summary>
    /// MagicNumbers can be found here: https://www.garykessler.net/library/file_sigs.html
    /// </summary>
    public abstract class FileFormatDescriptor
    {
        protected FileFormatDescriptor(string typeName)
        {
            TypeName = typeName;
            Initialize();
            MaxHeaderLength = Headers.Count > 0 ? Headers.Max(m => m.Length) : 0;
            MaxTrailerLength = Trailers.Count > 0 ? Trailers.Max(m => m.Length) : 0;
        }

        protected abstract void Initialize();
        protected HashSet<string> Extensions { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        protected List<byte[]> Headers { get; } = [];
        protected int MaxHeaderLength { get; }
        protected List<byte[]> Trailers { get; } = [];
        protected int MaxTrailerLength { get; }
        protected string TypeName { get; set; }
        public bool IsIncludedExtension(string extension) => Extensions.Contains(extension);

        public FileFormatValidationResult Validate(IFormFile file)
        {
            using Stream stream = file.OpenReadStream();
            
            Span<byte> headerBytes = stackalloc byte[MaxHeaderLength];
            Span<byte> trailerBytes = stackalloc byte[MaxTrailerLength];
            
            stream.ReadExactly(headerBytes);
            
            foreach (byte[] headerMagicNumber in Headers)
            {
                if (headerBytes[..headerMagicNumber.Length].SequenceCompareTo(headerMagicNumber) == 0)
                {
                    if (Trailers.Count > 0)
                    {
                        //move stream to end to we can check "trailers"
                        stream.Position = stream.Length - MaxTrailerLength;

                        stream.ReadExactly(trailerBytes);
                        foreach (byte[] trailerMagicNumber in Trailers)
                        {
                            if (trailerBytes[..trailerMagicNumber.Length].SequenceCompareTo(trailerMagicNumber) == 0)
                            {
                                return new FileFormatValidationResult(true, FileFormatStatusType.GENUINE, $"{FileFormatStatusType.GENUINE} {TypeName}");
                            }
                        }
                    }
                    else
                    {
                        return new FileFormatValidationResult(true, FileFormatStatusType.GENUINE, $"{FileFormatStatusType.GENUINE} {TypeName}");
                    }
                }
            }

            return new FileFormatValidationResult(false, FileFormatStatusType.FAKE, $"{FileFormatStatusType.FAKE} {TypeName}!");
        }
    }
}
