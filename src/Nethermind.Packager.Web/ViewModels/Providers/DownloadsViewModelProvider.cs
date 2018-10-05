using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nethermind.Packager.Core.Services;
using Nethermind.Packager.Core.Services.Dto;

namespace Nethermind.Packager.Web.ViewModels.Providers
{
    public class DownloadsViewModelProvider : IDownloadsViewModelProvider
    {
        private readonly IPackageLoader _packageLoader;
        private readonly ILogger<DownloadsViewModelProvider> _logger;

        public DownloadsViewModelProvider(IPackageLoader packageLoader,
            ILogger<DownloadsViewModelProvider> logger)
        {
            _packageLoader = packageLoader;
            _logger = logger;
        }

        public async Task<DownloadsViewModel> GetAsync()
        {
            var releases = new List<ReleaseViewModel>();
            var order = 1;
            try
            {
                var packages = await _packageLoader.GetAllAsync()
                    .ContinueWith(t => t.Result.GroupBy(p => p.Release));
            
                foreach (var release in packages)
                {
                    releases.Add(CreateRelease(release.Key, order++, release.ToList()));
                }

                return new DownloadsViewModel
                {
                    Releases = releases
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }

            return new DownloadsViewModel();
        }

        private ReleaseViewModel CreateRelease(string release, int order, IList<PackageDto> packages)
        {
            var viewModel = new ReleaseViewModel
            {
                Title = release,
                Description = $"{release} description...",
                Type = release,
                Order = order,
                Platforms = new List<PlatformViewModel>
                {
                    CreatePlatform("Linux", 1, packages.Where(p => p.Platform.Equals("linux"))),
                    CreatePlatform("Mac", 1, packages.Where(p => p.Platform.Equals("darwin"))),
                    CreatePlatform("Windows", 1, packages.Where(p => p.Platform.Equals("windows")))
                }
            };
            
            return viewModel;
        }

        private PlatformViewModel CreatePlatform(string name, int order, IEnumerable<PackageDto> packages)
            => new PlatformViewModel
            {
                Name = name,
                Order = order,
                Packages = packages
                    .OrderByDescending(p => p.Version)
                    .Select(p => new PackageViewModel
                {
                    Name = p.Name,
                    Arch = p.Arch,
                    Size = p.Size,
                    Commit = p.Commit,
                    Kind = p.Kind,
                    Url = p.Url,
                    Checksum = p.Checksum,
                    PublishedAt = p.PublishedAt,
                    Signature = p.Signature
                })
            };
    }
}