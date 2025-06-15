namespace Skeleton.API.Core.Validation.File
{
    public class Document : FileFormatDescriptor
    {
        public Document() : base("DOCUMENT FILE") { }

        protected override void Initialize()
        {
            Extensions.UnionWith([".doc", ".docx"]); //, ".xls", ".xlsx"]);
            Headers.AddRange([
                [0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1], //.doc and such
                [0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x06, 0x00] //.docx and such
            ]);
        }
    }
}
