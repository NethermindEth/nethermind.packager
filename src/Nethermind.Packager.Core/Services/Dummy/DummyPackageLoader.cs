using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nethermind.Packager.Core.Services.Dto;

namespace Nethermind.Packager.Core.Services.Dummy
{
    // geth-android-all-1.8.17-unstable-26a37c53.aar
    public class DummyPackageLoader : IPackageLoader
    {
        private static readonly Random Random = new Random();

        public async Task<IEnumerable<PackageDto>> GetAllAsync()
        {
            await Task.CompletedTask;
            return new List<PackageDto>
            {
                Create(),
                Create(),
                Create(),
                Create(),
                Create(),
                Create()
            };
        }

        private PackageDto Create()
            => new PackageDto
            {
                Name = $"Nethermind_{Random.Next(1, 10)}.{Random.Next(1, 10)}.{Random.Next(1, 10)}",
                Arch = "X64",
                Commit = Guid.NewGuid().ToString("N").Substring(0, 6),
                Size = (long) (Random.NextDouble() * Random.Next(100000, 150000)),
                Checksum = Guid.NewGuid().ToString("N").Substring(0, 6),
                Kind = "Archive",
                Signature = Guid.NewGuid().ToString("N").Substring(0, 6),
                PublishedAt = DateTime.UtcNow.AddDays(-1 * Random.Next(-20, 0)),
                Url = "http://google.com",
                Platform = "Linux",
                Release = "stable"
            };
    }
}