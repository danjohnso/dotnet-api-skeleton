﻿namespace Skeleton.API.Core.Validation.File
{
    public class Png : FileFormatDescriptor
    {
        public Png() : base("PNG FILE") { }

        protected override void Initialize()
        {
            Extensions.Add(".png");
            Headers.Add([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);
        }
    }
}
