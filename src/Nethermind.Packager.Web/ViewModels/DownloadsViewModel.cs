using System.Collections.Generic;
using System.Linq;

namespace Nethermind.Packager.Web.ViewModels
{
    public class DownloadsViewModel
    {
        public ReleaseViewModel LatestRelease { get; set; }
        public IEnumerable<ReleaseViewModel> Releases { get; set; } = new List<ReleaseViewModel>();
        public IEnumerable<SignaturesViewModel> Signatures { get; set; } = new List<SignaturesViewModel>();
    }
}