using System.Threading.Tasks;

namespace Nethermind.Packager.Core.Services.Storage
{
    public interface IStorageClient
    {
        Task<string> GetContentAsync();
    }
}