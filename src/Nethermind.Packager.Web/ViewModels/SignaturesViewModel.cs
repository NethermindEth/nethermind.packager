using System.Collections.Generic;

namespace Nethermind.Packager.Web.ViewModels
{
    public class SignaturesViewModel
    {
        public string Type { get; set; }
        public IEnumerable<SignatureViewModel> Signatures { get; set; } = new List<SignatureViewModel>();
    }
}