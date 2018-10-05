using System.Collections.Generic;

namespace Nethermind.Packager.Web.ViewModels
{
    public class PlatformViewModel
    {
        public int Order { get; set; }
        public string Name { get; set; }
        public IEnumerable<PackageViewModel> Packages { get; set; }
    }
}