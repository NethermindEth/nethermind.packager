using System.Threading.Tasks;

namespace Nethermind.Packager.Web.ViewModels.Providers
{
    public interface IDownloadsViewModelProvider
    {
        Task<DownloadsViewModel> GetAsync();
    }
}