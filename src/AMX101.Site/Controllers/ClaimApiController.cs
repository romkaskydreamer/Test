using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AMX101.Dto.Enitites;
using AMX101.Dto.Models;
using AMX101.Site.Models;
using AMX101.Site.Services;
using Auth0.AuthenticationApi.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using AMX101.Site.Configuration;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace AMX101.Site.Controllers
{
    [Route("api/claims")]
    public class ClaimApiController : Controller
    {
        private static IClaimService _claimService;
        private static IPostcodeService _postcodeService;
        private readonly ViewRender _view;
        private readonly Auth0Config _auth0Config;
        private readonly AsposeOptions _asposeOptions;
        private readonly IHostingEnvironment _env;

        public ClaimApiController(
            IClaimService claimService,
            IPostcodeService postcodeService,
            IOptions<Auth0Config> config,
            IOptions<AsposeOptions> asposeOptions,
            IHostingEnvironment env,
            ViewRender view)
        {
            _claimService = claimService;
            _postcodeService = postcodeService;
            _view = view;
            _auth0Config = config.Value;
            _asposeOptions = asposeOptions.Value;
            _env = env;
        }

        [HttpGet]
        [Route("{region}/{postcode}")]
        public IEnumerable<PopulatedClaim> GetPopulatedClaims([FromRoute] string postcode, [FromRoute] string region)
        {
            var isValid = _postcodeService.IsValidPostcode(postcode, region);

            if (!isValid) return Enumerable.Empty<PopulatedClaim>();

            var claims = _claimService.GetClaims(region);

            if (region == "sng")
            {
                return _claimService.PopulateSummedValues(claims, postcode, region);
            }
            return _claimService.PopulateValues(claims, postcode, region);
        }

        [HttpGet]
        [Route("{region}/static")]
        public IEnumerable<StaticClaim> GetStaticClaims([FromRoute] string region)
        {
            return _claimService.GetStaticClaims(region);
        }

        [HttpGet]
        [Route("pdf")]
        public async Task<IActionResult> ExportPdf(
            string postcode,
            double lat,
            double lng,
            int[] staticClaimIds,
            int[] claimIds,
            Industry industry, string region)
        {
            var sources = _claimService
                .GetSources(region)
                .OrderBy(a => a.Id)
                .ToDictionary(a => a.Id, a => a.Text);

            var pdfModel = new SingleViewPdfViewModel()
            {
                Postcode = postcode,
                Lat = lat,
                Lng = lng,
                Industry = industry,
                MapsUrl = await _claimService.GetPathToPostalCodeImage(postcode, region),
                ImageDomain = _asposeOptions.ImageUrl,
                Sources = sources
            };

            if (staticClaimIds != null && staticClaimIds.Any())
            {
                pdfModel.StaticClaims = _claimService.GetStaticClaims(staticClaimIds, region).ToList();
            }

            if (claimIds != null && claimIds.Any())
            {
                var claims = _claimService.GetClaims(claimIds, region);
                pdfModel.Claims = _claimService.PopulateValues(claims, postcode, region).ToList();
            }

            var html = _view.Render(@"Pdf\SingleView", pdfModel);

            var authClient = new Auth0.AuthenticationApi.AuthenticationApiClient("mudbath.au.auth0.com");

            var authReq = new AuthenticationRequest()
            {
                ClientId = _auth0Config.ClientId,
                Username = _auth0Config.Username,
                Password = _auth0Config.Password,
                Connection = "Username-Password-Authentication",
                GrantType = "password",
                Scope = "openid"
            };
            var resp = await authClient.AuthenticateAsync(authReq);

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(_asposeOptions.BaseUrl)
            };

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", resp.IdToken);

            var content = JsonConvert.SerializeObject(new { Html = html });

            var httpResp = await httpClient.PostAsync(
                "api/pdf/generate",
                new StringContent(content, Encoding.UTF8, "application/json"));

            if (!httpResp.IsSuccessStatusCode)
            {
                var error = httpResp.Content.ReadAsStringAsync();
                Console.WriteLine(error);
                httpResp.EnsureSuccessStatusCode();
            }

            var pdf = await httpResp.Content.ReadAsStreamAsync();

            return File(pdf, "application/pdf", $"Export-{postcode}-{DateTime.Now.ToString("dd-MM-yyy HH:mm")}.pdf");
        }

        [HttpGet]
        [Route("image")]
        public async Task<IActionResult> ExportImage(
            string postcode,
            int[] staticClaimIds, 
            int[] claimIds, string region)
        {
            var tileClaims = new List<TileViewModel>();

            if (staticClaimIds != null && staticClaimIds.Any())
            {
                var staticClaims = _claimService
                    .GetStaticClaims(staticClaimIds, region)
                    .Select(x => new TileViewModel()
                    {
                        Value = x.Value,
                        ImagePath = x.ImagePath,
                        Heading = x.Heading,
                        FormattedText = x.ClaimText,
                        ImageDomain = _asposeOptions.ImageUrl,
                        SourceId = x.SourceId
                    })
                    .ToList();

                tileClaims.AddRange(staticClaims);
            }

            if (claimIds != null && claimIds.Any())
            {
                var claims = _claimService.GetClaims(claimIds, region);

                var popClaims = _claimService
                    .PopulateValues(claims, postcode, region)
                    .Select(x => new TileViewModel()
                    {
                        Heading = x.ClaimName,
                        Value = x.ClaimValue,
                        ImagePath = x.ImagePath,
                        FormattedText = x.FormattedClaimText,
                        ImageDomain = _asposeOptions.ImageUrl,
                        SourceId = x.SourceId
                    })
                    .ToList();

                tileClaims.AddRange(popClaims);
            }
            
            var sourceText = _claimService
                .GetSources(region)
                .Select(a => $"{a.Id}. {a.Text}")
                .Aggregate(new StringBuilder(), (a, b) => a.AppendFormat("{0} ", b))
                .ToString()
                .Trim();

            var htmls = tileClaims
                .Select(a => _view.Render(@"Image\SingleView", a))
                .ToList();

            var authClient = new Auth0.AuthenticationApi.AuthenticationApiClient("mudbath.au.auth0.com");

            var authReq = new AuthenticationRequest()
            {
                ClientId = _auth0Config.ClientId,
                Username = _auth0Config.Username,
                Password = _auth0Config.Password,
                Connection = "Username-Password-Authentication",
                GrantType = "password",
                Scope = "openid"
            };

            var resp = await authClient.AuthenticateAsync(authReq);

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(_asposeOptions.BaseUrl)
            };

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", resp.IdToken);

            var content = JsonConvert.SerializeObject(new { Htmls = htmls, Width = 360 });

            var httpResp = await httpClient.PostAsync(
                "api/image/generate",
                new StringContent(content, Encoding.UTF8, "application/json"));

            if (!httpResp.IsSuccessStatusCode)
            {
                var error = httpResp.Content.ReadAsStringAsync();
                Console.WriteLine(error);
                httpResp.EnsureSuccessStatusCode();
            }

            var images = await httpResp.Content.ReadAsStringAsync();
            var imagesByteList = JsonConvert.DeserializeObject<List<byte[]>>(images);
            var zip = CreateZip(imagesByteList, sourceText);
            var fileName = $"image-export-{postcode}-{DateTime.Now.ToString("dd-MM-yyy HH:mm")}.zip";

            return File(zip, "application/zip", fileName);
        }

        [HttpGet("{region}/sources")]
        public IEnumerable<Source> GetSources([FromRoute] string region)
        {
            return _claimService.GetSources(region).OrderBy(x => x.Id);
        }

        [HttpGet("{region}/imageclaims")]
        public IEnumerable<IClaim> GetClaimsForImages([FromRoute] string region)
        {
            var claims = new List<IClaim>();
            claims.AddRange(_claimService.GetStaticClaims(region));
            return claims;
        }

        private Stream CreateZip(IList<byte[]> files,string sources)
        {
            var result = new MemoryStream();

            using (var archive = new ZipArchive(result, ZipArchiveMode.Create, true))
            {
                for (var i = 0; i < files.Count(); i++)
                {
                    var file = files[i];
                    var fileName = $"tile{i}.png";

                    this.CreateEntry(
                        archive,
                        fileName,
                        file);
                }

                this.CreateEntry(
                    archive,
                    "sources.txt",
                    sources);
            }

            result.Seek(0, SeekOrigin.Begin);

            return result;
        }

        private void CreateEntry(
            ZipArchive archive,
            string name,
            byte[] item)
        {
            var entry = archive.CreateEntry(name);

            using (var s = entry.Open())
            {
                s.Write(item, 0, item.Length);
            }
        }

        private void CreateEntry(
           ZipArchive archive,
           string name,
           string item)
        {
            var entry = archive.CreateEntry(name);

            using (var writer = new StreamWriter(entry.Open()))
            {
                writer.Write(item);
            }
        }
    }
}
