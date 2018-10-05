using System.Threading.Tasks;

namespace Nethermind.Packager.Core.Services.Storage
{
    public interface IStorageUploader
    {
        Task UploadAsync(byte[] bytes, string name);
    }
}