namespace Skeleton.API.Core.Validation.File
{
    public class Pdf : FileFormatDescriptor
    {
        public Pdf() : base("PDF FILE") { }

        protected override void Initialize()
        {
            Extensions.Add(".pdf");
            Headers.Add([0x25, 0x50, 0x44, 0x46]);
        }
    }
}
