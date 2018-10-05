using System.Collections.Generic;

namespace Nethermind.Packager.Web.ViewModels
{
    public class ReleaseViewModel
    {
        public int Order { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public IEnumerable<PlatformViewModel> Platforms { get; set; }
    }
}