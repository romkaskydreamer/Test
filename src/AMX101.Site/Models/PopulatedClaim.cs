using AMX101.Dto.Models;

namespace AMX101.Site.Models
{
    public class PopulatedClaim
    {
        public int Id { get; set; }
        public int? SourceId { get; set; }
        public string ClaimName { get; set; }
        public string ClaimValue { get; set; }
        public string FormattedClaimText { get; set; }
        public Industry Industry { get; set; }
        public string Postcode { get; set; }
        public int Threshold { get; set; }
        public Type Type { get; set; }
        public string ImagePath { get; set; }
    }
}
