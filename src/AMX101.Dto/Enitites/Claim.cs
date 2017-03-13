using AMX101.Dto.Models;

namespace AMX101.Dto.Enitites
{
    public class Claim: IClaim
    {
        public int Id { get; set; }
        public int? SourceId { get; set; }
        public string Heading { get; set; }
        public string ClaimText { get; set; }
        public Industry Industry { get; set; }
        public Type Type { get; set; }
        public string ImagePath { get; set; }
    }
}
