using System.Linq;
using Microsoft.Extensions.Options;
using PgpCore;

namespace Nethermind.Packager.Core.Services.Validators.Pgp
{
    public class PgpPackageValidator : IPackageValidator
    {
        private readonly IOptions<AccessOptions> _validatorOptions;

        public PgpPackageValidator(IOptions<AccessOptions> validatorOptions)
        {
            _validatorOptions = validatorOptions;
        }

        //TODO: PGP validator
        public bool IsValid(byte[] packageBytes, byte[] signatureBytes, string apiKey)
        {
            var user = _validatorOptions.Value.Users
                .SingleOrDefault(g => g.Value.SingleOrDefault(u => u.ApiKey == apiKey) != null);
            if (string.IsNullOrWhiteSpace(user.Key))
            {
                return false;
            }

            using (var pgp = new PGP())
            {
                return signatureBytes != null && signatureBytes.Any();
            }
        }
    }
}