using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nethermind.Packager.Core.Services;

namespace Nethermind.Packager.Web.Controllers
{
    [Route("files")]
    public class FilesController : Controller
    {
        private readonly IPackageUploader _packageUploader;

        public FilesController(IPackageUploader packageUploader)
        {
            _packageUploader = packageUploader;
        }
       
        [HttpPost]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [RequestSizeLimit(350_000_000)]
        public async Task<ActionResult> Post(List<IFormFile> files, string apiKey)
        {
            await _packageUploader.UploadAsync(files, apiKey);

            return NoContent();
        }
    }
}
