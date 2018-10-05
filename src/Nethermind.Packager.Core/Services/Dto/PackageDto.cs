using System;

namespace Nethermind.Packager.Core.Services.Dto
{
    public class PackageDto
    {
        public string Name { get; set; }
        public string Commit { get; set; }
        public string Kind { get; set; }
        public string Arch { get; set; }
        public long Size { get; set; }
        public DateTime PublishedAt { get; set; }
        public string Signature { get; set; }
        public string Checksum { get; set; }
        public string Platform { get; set; }
        public string Version { get; set; }
        public string Release { get; set; }
        public string Url { get; set; }
    }
}