using System.Collections.Generic;
using System.Threading.Tasks;
using Nethermind.Packager.Core.Services.Dto;

namespace Nethermind.Packager.Core.Services
{
    public interface IPackageLoader
    {
        Task<IEnumerable<PackageDto>> GetAllAsync();
    }
}