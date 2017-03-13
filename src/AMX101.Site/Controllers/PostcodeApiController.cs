using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AMX101.Site.Models;
using AMX101.Site.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace AMX101.Site.Controllers
{
    [Route("api/postcode")]
    public class PostcodeApiController : Controller
    {
        private readonly IPostcodeService _postcodeService;

        public PostcodeApiController(IPostcodeService postcodeService)
        {
            _postcodeService = postcodeService;
        }
        [HttpGet]
        [Route("search")]
        public IEnumerable<AutocompleteResult> Search([FromQuery]string query, [FromQuery]string region)
        {
            if (!string.IsNullOrEmpty(query))
            {
                return _postcodeService.Search(query, region);
            }
            return new List<AutocompleteResult>();
            
        }

        [HttpGet]
        [Route("validate")]
        public bool IsValid([FromQuery]string query, [FromQuery]string region)
        {
            return _postcodeService.IsValidPostcode(query, region);
        }
    }
}
