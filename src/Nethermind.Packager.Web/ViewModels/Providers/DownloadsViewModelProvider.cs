using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethermind.Packager.Core.Services;
using Nethermind.Packager.Core.Services.Dto;
using Nethermind.Packager.Core.Services.Options;

namespace Nethermind.Packager.Web.ViewModels.Providers
{
    public class DownloadsViewModelProvider : IDownloadsViewModelProvider
    {
        private readonly IPackageLoader _packageLoader;
        private readonly IOptions<AccessOptions> _accessOptions;
        private readonly IOptions<PackageOptions> _packageOptions;
        private readonly ILogger<DownloadsViewModelProvider> _logger;

        public DownloadsViewModelProvider(IPackageLoader packageLoader,
            IOptions<AccessOptions> accessOptions,
            IOptions<PackageOptions> packageOptions,
            ILogger<DownloadsViewModelProvider> logger)
        {
            _packageLoader = packageLoader;
            _accessOptions = accessOptions;
            _packageOptions = packageOptions;
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

                foreach (var package in packages)
                {
                    releases.Add(CreateRelease(package.Key, order++, package.ToList()));
                }

                return new DownloadsViewModel
                {
                    Releases = releases,
                    Signatures = CreateSignatures()
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }

            return new DownloadsViewModel();
        }

        private ReleaseViewModel CreateRelease(string releaseName, int order, IList<PackageDto> packages)
        {
            var release = _packageOptions.Value.Releases
                .SingleOrDefault(r => r.Key.Equals(releaseName, StringComparison.InvariantCultureIgnoreCase));
            
            var viewModel = new ReleaseViewModel
            {
                Title = release.Key,
                Description = release.Value,
                Type = release.Key,
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
                Repository = _packageOptions.Value.Repository,
                Packages = packages
                    .OrderByDescending(p => p.Version)
                    .Select(p => new PackageViewModel
                    {
                        Name = p.Name,
                        Arch = p.Arch,
                        Size = p.Size,
                        Commit = p.Commit.Substring(0, 6),
                        Kind = p.Kind,
                        Url = p.Url,
                        SignatureUrl = $"{p.Url}.{_packageOptions.Value.SignatureExtension}",
                        Checksum = p.Checksum,
                        PublishedAt = p.PublishedAt,
                        Signature = p.Signature
                    })
            };

        private IEnumerable<SignaturesViewModel> CreateSignatures()
        {
            foreach (var group in _accessOptions.Value.Users)
            {
                yield return new SignaturesViewModel
                {
                    Type = group.Key,
                    Signatures = group.Value.Select(u => new SignatureViewModel
                    {
                        Name = u.Name,
                        UniqueId = u.UniqueId,
                        PublicKey = u.PublicKey,
                        Fingerprint = u.Fingerprint
                    })
                };
            }
        }
    }
}