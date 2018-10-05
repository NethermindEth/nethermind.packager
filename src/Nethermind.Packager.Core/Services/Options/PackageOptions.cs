using System.Collections.Generic;

namespace Nethermind.Packager.Core.Services.Options
{
    public class PackageOptions
    {
        public string DefaultRelease { get; set; }
        public IDictionary<string, string> Extensions { get; set; }
        public IEnumerable<string> Releases { get; set; }
        public string SignatureExtension { get; set; }
        public bool ValidateSignature { get; set; }
    }
}