namespace Nethermind.Packager.Core.Services.Validators
{
    public interface IPackageValidator
    {
        bool IsValid(byte[] packageBytes, byte[] signatureBytes, string from);
    }
}