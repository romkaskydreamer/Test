using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AMX101.Site.Models;
using AMX101.Dto.Enitites;
using AMX101.Dto.Models;
using Type = AMX101.Dto.Models.Type;
using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using AMX101.Site.Configuration;
using Microsoft.Extensions.Options;

namespace AMX101.Site.Services
{
    public interface IClaimService
    {
        Claim GetClaim(int id, string region);
        IEnumerable<StaticClaim> GetStaticClaims(string region);
        IEnumerable<StaticClaim> GetStaticClaims(IEnumerable<int> ids, string region);
        IEnumerable<Claim> GetClaims(string region);
        IEnumerable<Claim> GetClaims(IEnumerable<int> ids, string region);
        IEnumerable<PopulatedClaim> PopulateValues(IEnumerable<Claim> claims, string postcode, string region);
        ComparisonPostcode ComparePostcode(string postcode, Industry industry, string region);
        Task<string> GetPathToPostalCodeImage(string postCode, string region);
        IEnumerable<Source> GetSources(string region);
        IEnumerable<PopulatedClaim> PopulateSummedValues(IEnumerable<Claim> claims, string postcode, string region);

    }
    public class ClaimService : IClaimService
    {
        private readonly IHostingEnvironment _env;
        private readonly AsposeOptions _asposeOptions;
        private readonly IDataRepository _repository;


        public ClaimService(IHostingEnvironment environment, IOptions<AsposeOptions> asposeOptions, IDataRepository repository)
        {
            _env = environment;
            _asposeOptions = asposeOptions.Value;
            _repository = repository;
        }

        public Claim GetClaim(int id, string region)
        {
            return _repository.GetClaim(id, region);
        }

        public IEnumerable<StaticClaim> GetStaticClaims(string region)
        {
            return _repository.GetStaticClaims(region);

        }

        public IEnumerable<StaticClaim> GetStaticClaims(IEnumerable<int> ids, string region)
        {
            return _repository.GetStaticClaims(ids, region);

        }

        public IEnumerable<Claim> GetClaims(string region)
        {
            return _repository.GetClaims(region);
        }

        public IEnumerable<Claim> GetClaims(IEnumerable<int> ids, string region)
        {
            return _repository.GetClaims(ids, region);
        }

        public IEnumerable<PopulatedClaim> PopulateValues(IEnumerable<Claim> claims, string postcode, string region)
        {
            var postcodeClaims = _repository.GetClaimValues(postcode, region);
            var populatedClaims = new List<PopulatedClaim>();
            foreach (var claim in claims)
            {
                var popClaim = MapToPopulatedClaim(claim, postcodeClaims, region);

                if (!string.IsNullOrEmpty(popClaim?.Postcode) && !string.IsNullOrEmpty(popClaim.ClaimName))
                    if (!string.IsNullOrEmpty(popClaim?.Postcode) && !string.IsNullOrEmpty(popClaim.ClaimName))
                    {
                        populatedClaims.Add(popClaim);
                    }
            }

            return populatedClaims;
        }

        // NOTE: This is different between versions
        private PopulatedClaim MapToPopulatedClaim(Claim claim, IEnumerable<ClaimValue> postcodeClaims, string region)
        {
            var matchedClaims = postcodeClaims.Where(x => x.ClaimId == claim.Id).OrderBy(x => x.Order).ToList();
            if (!matchedClaims.Any()) return null;

            var popClaim = new PopulatedClaim()
            {
                Postcode = matchedClaims.FirstOrDefault().Postcode,
                Id = claim.Id,
                ClaimName = claim.Heading,
                ClaimValue = matchedClaims.FirstOrDefault().Value?.ToString("##,##0") ?? string.Empty,
                Threshold = matchedClaims.FirstOrDefault().Threshold,
                SourceId = claim.SourceId,
                Type = claim.Type,
                Industry = claim.Industry,
                ImagePath = !string.IsNullOrEmpty(claim.ImagePath) ? claim.ImagePath : "img/icon1.png"
            };
            var showHardcodedValue = false;
            if (region != "aus" && popClaim.Type == Type.Static)
            {
                //if its not aus then we show the hardcoded value
                popClaim.ClaimValue = "117.8 Million";
                showHardcodedValue = true;
            }
            else if (region == "aus" && popClaim.Type == Type.Static && matchedClaims.FirstOrDefault().Value < 500)
            {
                //if it is aus but there isn't a big enough value, show the hardcoded value
                popClaim.ClaimValue = "117.8 Million";
                showHardcodedValue = true;
            }
            if (!string.IsNullOrEmpty(claim.ClaimText))
            {
                var splitText = claim.ClaimText.Split(new string[] { "[VALUE]" }, StringSplitOptions.None).ToList();

                var formattedClaimText = splitText.FirstOrDefault();

                try
                {
                    if (showHardcodedValue)
                    {
                        formattedClaimText = $"American Express has over 117.8 million Cards in Force world wide";
                    }
                    else
                    {
                        for (var i = 0; i < matchedClaims.Count; i++)
                        {
                            formattedClaimText += $"{matchedClaims[i].Value?.ToString("##,##0") ?? string.Empty} {splitText[i + 1]?.TrimStart(' ') ?? string.Empty} ";
                        }
                    }

                }
                catch (Exception ex)
                {
                    //TODO: improve. formatting fails sometimes
                }

                popClaim.FormattedClaimText = formattedClaimText;
            }
            else
            {
                popClaim.FormattedClaimText = string.Empty;
            }

            return popClaim;
        }

        public ComparisonPostcode ComparePostcode(string postcode, Industry industry, string region)
        {
            var claims = _repository.GetClaims(region).Where(x => x.Industry == industry || x.Type == Type.Static).ToList();

            var postcodeValues = _repository.GetClaimValues(postcode, region);

            var compPostcode = new ComparisonPostcode()
            {
                Postcode = postcode
            };

            foreach (var vals in postcodeValues.Where(x => x.Order == 0 && claims.Any(y => y.Id == x.ClaimId)))
            {
                var claim = claims.FirstOrDefault(x => x.Id == vals.ClaimId);
                switch (claim.Type)
                {
                    case Type.Static:
                        compPostcode.Cards = Convert.ToInt32(vals.Value);
                        break;
                    case Type.DynamicPrimary:
                        compPostcode.Transactions = vals.Value ?? 0;
                        break;
                    case Type.DynamicSecondary:
                        compPostcode.MerchantSpend = vals.Value ?? 0;
                        break;
                }
            }
            return compPostcode;

        }

        public async Task<string> GetPathToPostalCodeImage(string postCode, string region)
        {
            var targetPath = Path.Combine(_env.WebRootPath, "mapimages");
            var filePath = Path.Combine(targetPath, $"{postCode}.png");
            // If the file does not exist we need to make it
            if (!File.Exists(filePath))
            {
                await WritePostCodeImage(postCode, region);// Write the image to disk and then continue to serve it
            }

            // Else we just return the url to access this file from the web
            return $"mapimages/{postCode}.png";

        }

        public async Task<bool> WritePostCodeImage(string postCode, string region)
        {
            // Create the path for 
            var targetPath = Path.Combine(_env.WebRootPath, "mapimages");
            var filePath = Path.Combine(targetPath, $"{postCode}.png");

            if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);
            var center = string.Empty;
            switch (region)
            {
                case "aus":
                    center = "Australia,";
                    break;
                case "nz":
                    center = "New Zealand,";
                    break;
                case "sng":
                    center = "Singapore,";
                    break;
            }

            string url = $"https://maps.googleapis.com/maps/api/staticmap?key=AIzaSyCKDgyXKZyjoieQyRUFGDPXPVHtcCEKYgI&center={center}{postCode}&zoom=14&size=500x219&scale=2&maptype=roadmap&style=feature:all|element:geometry|color:0x174972&style=feature:all|element:geometry.stroke|weight:1.07|lightness:7&style=feature:all|element:labels.text|visibility:off&style=feature:administrative|visibility:off&style=element:labels|visibility:off&style=feature:landscape|element:all|color:0x1b68b2&style=feature:landscape.man_made|element:all|visibility:off&style=feature:poi|element:geometry|lightness:-7&style=feature:road|element:all|visibility:simplified&style=feature:road|element:labels|visibility:off&style=feature:transit|element:all|visibility:off&style=feature:water|element:all|lightness:-24";
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = await client.GetAsync(url))
            using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
            using (Stream memStream = new MemoryStream())
            {
                await streamToReadFrom.CopyToAsync(memStream);

                // Init the bytes stream to the same size of the bytes to be read
                var bytes = new byte[memStream.Length];

                memStream.Position = 0;

                // Write the file to disk and then pass that file uri to the model to be rendered 
                // TODO NB!!! NEED TO CHECK IF THE FILE EXISTS BEFORE writing a new one
                targetPath = Path.Combine(_env.WebRootPath, "mapimages");
                if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);

                // The file to write is the name of the post code that the pdf is being generated for
                var fileToWriteTo = filePath;

                using (Stream streamToWriteTo = System.IO.File.Open(fileToWriteTo, FileMode.Create))
                {
                    memStream.Position = 0;
                    // Copy the data in the memory stream into the a file.
                    await memStream.CopyToAsync(streamToWriteTo);
                }



                // Clear the response
                response.Content = null;
            }

            // Check if the file exists now to make sure that it saved
            return File.Exists(filePath);

        }

        public IEnumerable<Source> GetSources(string region)
        {
            return _repository.GetSources(region);
        }


        //these are for singapore only - amalgamating all the postcodes into a subset based on the first to characters
        public IEnumerable<PopulatedClaim> PopulateSummedValues(IEnumerable<Claim> claims, string postcode, string region)
        {
            var postcodePrefix = postcode.Substring(0, 2);
            var postcodeClaims = _repository.GetClaimValuesByPrefix(postcodePrefix, region);
            var populatedClaims = new List<PopulatedClaim>();

            foreach (var claim in claims)
            {
                var popClaim = MapToSummedPopulatedClaim(claim, postcodeClaims);

                if (!string.IsNullOrEmpty(popClaim?.Postcode) && !string.IsNullOrEmpty(popClaim.ClaimName))
                {
                    populatedClaims.Add(popClaim);
                }
            }

            return populatedClaims;

        }

        private PopulatedClaim MapToSummedPopulatedClaim(Claim claim, IEnumerable<ClaimValue> postcodeClaims)
        {
            var matchedClaims = postcodeClaims.Where(x => x.ClaimId == claim.Id).OrderBy(x => x.Order).ToList();
            if (!matchedClaims.Any()) return null;

            var sumVal = matchedClaims.Where(x => x.Order == 0).Sum(x => x.Value);

            var popClaim = new PopulatedClaim()
            {
                Postcode = matchedClaims.FirstOrDefault().Postcode,
                Id = claim.Id,
                ClaimName = claim.Heading,
                ClaimValue = sumVal?.ToString("##,##0") ?? string.Empty,
                Threshold = matchedClaims.FirstOrDefault().Threshold,
                SourceId = claim.SourceId,
                Type = claim.Type,
                Industry = claim.Industry,
                ImagePath = !string.IsNullOrEmpty(claim.ImagePath) ? claim.ImagePath : "img/icon1.png"
            };
            if (popClaim.Type == Type.Static)
            {
                popClaim.ClaimValue = "117.8 Million";
            }
            if (!string.IsNullOrEmpty(claim.ClaimText))
            {
                var splitText = claim.ClaimText.Split(new string[] { "[VALUE]" }, StringSplitOptions.None).ToList();

                var formattedClaimText = splitText.FirstOrDefault();

                try
                {
                    for (var i = 0; i < matchedClaims.Count; i++)
                    {
                        if (i == 0 && popClaim.Type == Type.Static)
                        {
                            formattedClaimText +=
                                $"117.8 {splitText[i + 1]?.TrimStart(' ') ?? string.Empty} ";
                        }
                        else
                        {
                            formattedClaimText +=
                                $"{sumVal?.ToString("##,##0") ?? string.Empty} {splitText[i + 1]?.TrimStart(' ') ?? string.Empty} ";
                        }
                    }
                }
                catch (Exception ex)
                {
                    //TODO: improve. formatting fails sometimes
                }

                popClaim.FormattedClaimText = formattedClaimText;
            }
            else
            {
                popClaim.FormattedClaimText = string.Empty;
            }

            return popClaim;
        }
    }

}
