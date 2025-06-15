namespace Skeleton.API.Core.Validation.File
{
    public class Jpg : FileFormatDescriptor
    {
        public Jpg() : base("JPG FILE") { }

        protected override void Initialize()
        {
            //jpg technically has 2 byte sequence opener (FF D8) and 2 byte sequence trailer (FF D9)
            //the good jpgs should have 3 byte opener we will use 0xFF, 0xD8, 0xFF

            Extensions.UnionWith([".jpeg", ".jpg", ".png"]);
            Headers.AddRange(
            [
                [0xFF, 0xD8, 0xFF],
            ]);

            Trailers.AddRange(
            [
                [0xFF, 0xD9],
            ]);
        }
    }
}
