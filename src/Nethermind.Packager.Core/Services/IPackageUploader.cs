using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Nethermind.Packager.Core.Services
{
    public interface IPackageUploader
    {
        Task UploadAsync(IReadOnlyCollection<IFormFile> files, string from);
    }
}