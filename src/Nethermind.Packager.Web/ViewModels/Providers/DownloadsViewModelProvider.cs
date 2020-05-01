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
    public class StringNumberComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            int compareResult;
            int xIndex = 0, yIndex = 0;
            int xIndexLast = 0, yIndexLast = 0;
            int xNumber, yNumber;
            int xLength = x.Length;
            int yLength = y.Length;

            do
            {
                bool xHasNextNumber = TryGetNextNumber(x, ref xIndex, out xNumber);
                bool yHasNextNumber = TryGetNextNumber(y, ref yIndex, out yNumber);

                if (!(xHasNextNumber && yHasNextNumber))
                {
                    return x.Substring(xIndexLast).CompareTo(y.Substring(yIndexLast));
                }

                xIndexLast = xIndex;
                yIndexLast = yIndex;

                compareResult = xNumber.CompareTo(yNumber);
            }
            while (compareResult == 0
                && xIndex < xLength
                && yIndex < yLength);

            return compareResult;
        }

        private bool TryGetNextNumber(string text, ref int startIndex, out int number)
        {
            number = 0;

            int pos = text.IndexOf('.', startIndex);
            if (pos < 0) pos = text.Length;

            if (!int.TryParse(text.Substring(startIndex, pos - startIndex), out number))
                return false;

            startIndex = pos + 1;

            return true;
        }
    }
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
                    LatestRelease = GetLatestRelease(releases),
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

        private ReleaseViewModel GetLatestRelease(IList<ReleaseViewModel> releases)
            => releases.FirstOrDefault(r => r.Type == _packageOptions.Value.Releases.First().Key);

        private ReleaseViewModel CreateRelease(string releaseName, int order, IList<PackageDto> packages)
        {
            var release = _packageOptions.Value.Releases
                .SingleOrDefault(r => r.Key.Equals(releaseName, StringComparison.InvariantCultureIgnoreCase));
            packages = packages.OrderByDescending(p => p.PublishedAt).ToList();
            
            var viewModel = new ReleaseViewModel
            {
                Title = release.Key,
                Description = release.Value,
                Type = release.Key,
                Order = order,
                Platforms = new List<PlatformViewModel>
                {
                    CreatePlatform("Linux", 1, packages.Where(p => p.Platform.Equals("linux") && p.Name.Contains("nethermind"))),
                    CreatePlatform("Mac", 1, packages.Where(p => p.Platform.Equals("darwin") && p.Name.Contains("nethermind"))),
                    CreatePlatform("Windows", 1, packages.Where(p => p.Platform.Equals("windows") && p.Name.Contains("nethermind"))),
                    CreatePlatform("NDM-Linux", 1, packages.Where(p => p.Platform.Equals("linux") && p.Name.Contains("NDM"))),
                    CreatePlatform("NDM-Mac", 1, packages.Where(p => p.Platform.Equals("darwin") && p.Name.Contains("NDM"))),
                    CreatePlatform("NDM-Windows", 1, packages.Where(p => p.Platform.Equals("windows") && p.Name.Contains("NDM")))
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
                    .OrderByDescending(p => p.Version, new StringNumberComparer())
                    .Select(p => new PackageViewModel
                    {
                        Name = p.Name,
                        Arch = p.Arch,
                        Size = p.Size,
                        Commit = p.Commit?.Length >= 7 ?  p.Commit.Substring(0, 7) : p.Commit?.Substring(0, p.Commit?.Length ?? 0),
                        Kind = p.Kind,
                        Url = p.Url,
                        SignatureUrl = $"{p.Url}.{_packageOptions.Value.SignatureExtension}",
                        Stability = p.Stability,
                        Checksum = p.Checksum,
                        PublishedAt = p.PublishedAt,
                        Signature = p.Signature
                    }).Take(30)
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