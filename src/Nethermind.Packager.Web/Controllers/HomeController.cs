using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nethermind.Packager.Core.Services;
using Nethermind.Packager.Web.ViewModels;
using Nethermind.Packager.Web.ViewModels.Providers;

namespace Nethermind.Packager.Web.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private readonly IDownloadsViewModelProvider _downloadsViewModelProvider;

        public HomeController(IDownloadsViewModelProvider downloadsViewModelProvider)
        {
            _downloadsViewModelProvider = downloadsViewModelProvider;
        }
        
//        [HttpGet]
//        public IActionResult Index()
//        {
//            return View();
//        }
//
//        [HttpGet("install")]
//        public IActionResult Install()
//        {
//            return View();
//        }

        [HttpGet]
        public async Task<IActionResult> Downloads()
            => View(await _downloadsViewModelProvider.GetAsync());

        [HttpGet("privacy")]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
