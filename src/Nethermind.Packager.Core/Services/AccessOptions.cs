using System.Collections.Generic;

namespace Nethermind.Packager.Core.Services
{
    public class AccessOptions
    {
        public IDictionary<string, IEnumerable<User>> Users { get; set; }

        public class User
        {
            public string Type { get; set; }
            public string Name { get; set; }
            public string UniqueId { get; set; }
            public string PublicKey { get; set; }
            public string Fingerprint { get; set; }
            public string ApiKey { get; set; }
        }
    }
}