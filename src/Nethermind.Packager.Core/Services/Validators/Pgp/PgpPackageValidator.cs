using System.Linq;
using Microsoft.Extensions.Options;
using PgpCore;

namespace Nethermind.Packager.Core.Services.Validators.Pgp
{
    public class PgpPackageValidator : IPackageValidator
    {
        private readonly IOptions<ValidatorOptions> _validatorOptions;

        public PgpPackageValidator(IOptions<ValidatorOptions> validatorOptions)
        {
            _validatorOptions = validatorOptions;
        }

        //TODO: PGP validator
        public bool IsValid(byte[] packageBytes, byte[] signatureBytes, string @from)
        {
            if (!_validatorOptions.Value.Keys.ContainsKey(@from))
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