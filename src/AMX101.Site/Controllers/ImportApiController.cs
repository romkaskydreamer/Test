using AMX101.Site.Services;
using Microsoft.AspNetCore.Mvc;

namespace AMX101.Site.Controllers
{
    [Route("api/import")]
    public class ImportApiController : Controller
    {
        private readonly IImportService _importService;
        public ImportApiController(IImportService importService)
        {
            _importService = importService;
        }

        [HttpPost]
        [Route("staticclaims")]
        public IActionResult LoadStaticClaims(string path)
        {
            var errors = _importService.ImportStaticClaimsData(path);

            if (errors.Count > 0)
            {
                return Ok(errors);
            }
            return Content("Static claims successfully imported");
        }

        [HttpPost]
        [Route("claims")]
        public IActionResult LoadClaims(string claimsName, string postcodeName)
        {
            var errors = _importService.ImportClaimsData(claimsName, postcodeName);

            if (errors.Count > 0)
            {
                return Ok(errors);
            }
            return Content("Claims and postcode data successfully imported");
        }
    }
}