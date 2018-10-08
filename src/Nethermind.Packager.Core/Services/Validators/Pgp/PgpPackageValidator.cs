using System.Linq;
using Microsoft.Extensions.Options;
using PgpCore;

namespace Nethermind.Packager.Core.Services.Validators.Pgp
{
    public class PgpPackageValidator : IPackageValidator
    {
        private readonly IOptions<AccessOptions> _accessOptions;

        public PgpPackageValidator(IOptions<AccessOptions> accessOptions)
        {
            _accessOptions = accessOptions;
        }

        //TODO: PGP validator
        public bool IsValid(byte[] packageBytes, byte[] signatureBytes, string apiKey)
        {
            using (var pgp = new PGP())
            {
                return signatureBytes != null && signatureBytes.Any();
            }
        }
    }
}