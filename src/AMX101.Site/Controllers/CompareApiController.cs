using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AMX101.Dto.Models;
using AMX101.Site.Models;
using AMX101.Site.Services;
using Auth0.AuthenticationApi.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using AMX101.Site.Configuration;

namespace AMX101.Site.Controllers
{
    [Route("api/compare")]
    public class CompareApiController:Controller
    {
        private readonly IClaimService _claimService;
        private readonly ViewRender _view;
        private readonly Auth0Config _auth0Config;
        private readonly IHostingEnvironment _env;
        private readonly AsposeOptions _asposeOptions;

        public CompareApiController(
            IClaimService claimService, 
            ViewRender view,
            IOptions<Auth0Config> config, 
            IHostingEnvironment env, 
            IOptions<AsposeOptions> asposeOptions)
        {
            _claimService = claimService;
            _view = view;
            _auth0Config = config.Value;
            _env = env;
            _asposeOptions = asposeOptions.Value;
        }

        [HttpPost]
        [Route("savesvg")]
        public List<string> SaveSvg([FromBody]SvgModel model)
        {
            var cardGuid = Guid.NewGuid().ToString();
            var transGuid = Guid.NewGuid().ToString();
            var spendGuid = Guid.NewGuid().ToString();

            var targetPath = Path.Combine(_env.WebRootPath, "svgs");
            if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);

            System.IO.File.WriteAllText(Path.Combine(targetPath, cardGuid + ".svg"), model.CardSvg);
            System.IO.File.WriteAllText(Path.Combine(targetPath, transGuid + ".svg"), model.TransSvg);
            System.IO.File.WriteAllText(Path.Combine(targetPath, spendGuid + ".svg"), model.SpendSvg);

            var guids = new List<string>();
            guids.Add(cardGuid);
            guids.Add(transGuid);
            guids.Add(spendGuid);
            return guids;
        }

        [HttpGet]
        [Route("pdf")]
        public async Task<IActionResult> GeneratePdf(string[] postcodes, string[] svgGuids, Industry industry, string region)
        {
            var sources = _claimService
                .GetSources(region)
                .OrderBy(a => a.Id)
                .ToDictionary(a => a.Id, a => a.Text);

            var pdfModel = new ComparePdfViewModel()
            {
                Industry = industry,
                Sources = sources
            };

            var populatedPostcodes = new List<ComparisonPostcode>();

            foreach (var code in postcodes)
            {
                populatedPostcodes.Add(_claimService.ComparePostcode(code, industry, region));
            }

            pdfModel.Postcodes = populatedPostcodes;
            pdfModel.TotalCards = populatedPostcodes.Sum(x => x.Cards);
            pdfModel.TotalTransactions = populatedPostcodes.Sum(x => x.Transactions);
            pdfModel.TotalMerchantSpend = populatedPostcodes.Sum(x => x.MerchantSpend);

            pdfModel.ImageDomain = _asposeOptions.ImageUrl;

            var cardSvgPath = Path.Combine(_env.WebRootPath, "svgs", svgGuids[0] + ".svg");
            var transSvgPath = Path.Combine(_env.WebRootPath, "svgs", svgGuids[1] + ".svg");
            var spendSvgPath = Path.Combine(_env.WebRootPath, "svgs", svgGuids[2] + ".svg");

            pdfModel.CardSvg = System.IO.File.ReadAllText(cardSvgPath);
            pdfModel.TransSvg = System.IO.File.ReadAllText(transSvgPath);
            pdfModel.SpendSvg = System.IO.File.ReadAllText(spendSvgPath);
            
            var html = _view.Render(@"Pdf\CompareView", pdfModel);

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

            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(this._asposeOptions.BaseUrl);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", resp.IdToken);

            var content = JsonConvert.SerializeObject(new { Html = html });

            var httpResp = await httpClient.PostAsync("api/pdf/generate", new StringContent(content, Encoding.UTF8, "application/json"));

            if (!httpResp.IsSuccessStatusCode)
            {
                var error = httpResp.Content.ReadAsStringAsync();
                Console.WriteLine(error);
                httpResp.EnsureSuccessStatusCode();
            }

            var pdf = await httpResp.Content.ReadAsStreamAsync();

            System.IO.File.Delete(cardSvgPath);
            System.IO.File.Delete(transSvgPath);
            System.IO.File.Delete(spendSvgPath);

            return File(pdf, "application/pdf", $"comparison-{DateTime.Now.ToString("dd-MM-yyy HH:mm")}.pdf");
        }

        [HttpGet]
        [Route("{postcode}")]
        public ComparisonPostcode Compare([FromRoute] string postcode, [FromQuery]Industry industry, [FromQuery]string region)
        {
            return _claimService.ComparePostcode(postcode, industry, region);
        }

        [HttpGet]
        [Route("image")]
        public async Task<IActionResult> GenerateImage(string[] postcodes, string[] svgGuids, Industry industry, string region)
        {
            var sources = _claimService
                .GetSources(region)
                .OrderBy(a => a.Id)
                .ToDictionary(a => a.Id, a => a.Text);

            var pdfModel = new ComparePdfViewModel()
            {
                Industry = industry,
                Sources = sources
            };

            var populatedPostcodes = new List<ComparisonPostcode>();

            foreach (var code in postcodes)
            {
                populatedPostcodes.Add(_claimService.ComparePostcode(code, industry, region));
            }

            pdfModel.Postcodes = populatedPostcodes;
            pdfModel.TotalCards = populatedPostcodes.Sum(x => x.Cards);
            pdfModel.TotalTransactions = populatedPostcodes.Sum(x => x.Transactions);
            pdfModel.TotalMerchantSpend = populatedPostcodes.Sum(x => x.MerchantSpend);

            pdfModel.ImageDomain = _asposeOptions.ImageUrl;

            var cardSvgPath = Path.Combine(_env.WebRootPath, "svgs", svgGuids[0] + ".svg");
            var transSvgPath = Path.Combine(_env.WebRootPath, "svgs", svgGuids[1] + ".svg");
            var spendSvgPath = Path.Combine(_env.WebRootPath, "svgs", svgGuids[2] + ".svg");

            pdfModel.CardSvg = System.IO.File.ReadAllText(cardSvgPath);
            pdfModel.TransSvg = System.IO.File.ReadAllText(transSvgPath);
            pdfModel.SpendSvg = System.IO.File.ReadAllText(spendSvgPath);

            var htmls = new List<string>();

            var html = _view.Render(@"Image\CompareView", pdfModel);

            htmls.Add(html);

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

            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(_asposeOptions.BaseUrl);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", resp.IdToken);

            var content = JsonConvert.SerializeObject(new { Htmls = htmls, Width = 595 });

            var httpResp = await httpClient.PostAsync("api/image/generate", new StringContent(content, Encoding.UTF8, "application/json"));

            if (!httpResp.IsSuccessStatusCode)
            {
                var error = httpResp.Content.ReadAsStringAsync();
                Console.WriteLine(error);
                httpResp.EnsureSuccessStatusCode();
            }

            var bytes = await httpResp.Content.ReadAsStringAsync();

            var image = JsonConvert.DeserializeObject<List<byte[]>>(bytes);

            System.IO.File.Delete(cardSvgPath);
            System.IO.File.Delete(transSvgPath);
            System.IO.File.Delete(spendSvgPath);

            return File(image[0], "image/png", $"comparison-{DateTime.Now.ToString("dd-MM-yyy HH:mm")}.png");
        }
    }
}
