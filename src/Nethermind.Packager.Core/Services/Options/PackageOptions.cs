using System.Collections.Generic;

namespace Nethermind.Packager.Core.Services.Options
{
    public class PackageOptions
    {
        public IDictionary<string, string> Extensions { get; set; }
        public IDictionary<string, string> Releases { get; set; }
        public string Repository { get; set; }
        public string SignatureExtension { get; set; }
        public bool ValidateSignature { get; set; }
    }
}