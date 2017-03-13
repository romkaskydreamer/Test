using System.Collections.Generic;
using AMX101.Dto.Enitites;
using AMX101.Dto.Models;

namespace AMX101.Site.Models
{
    public class SingleViewPdfRequestModel
    {
        public string Postcode { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public List<int> ClaimIds { get; set; }
        public List<int> StaticClaimIds { get; set; }
    }

    public class SingleViewPdfViewModel
    {
        public string Postcode { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public List<PopulatedClaim> Claims { get; set; } = new List<PopulatedClaim>();
        public List<StaticClaim> StaticClaims { get; set; } = new List<StaticClaim>();
        public Industry Industry { get; set; }

        public string MapsUrl { get; set; }
        public string ImageDomain { get; set; }

        public IDictionary<int, string> Sources { get; set; }
    }
}
