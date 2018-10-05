using System.Collections.Generic;

namespace Nethermind.Packager.Web.ViewModels
{
    public class DownloadsViewModel
    {
        public IEnumerable<ReleaseViewModel> Releases { get; set; } = new List<ReleaseViewModel>();
    }
}