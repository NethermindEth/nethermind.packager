namespace Nethermind.Packager.Core.Services
{
    public interface IHashGenerator
    {
        string Hash(string input);
    }
}