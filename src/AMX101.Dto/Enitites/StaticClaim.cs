using AMX101.Dto.Models;

namespace AMX101.Dto.Enitites
{
    public class StaticClaim: IClaim
    {
        public int Id { get; set; }
        public int? SourceId { get; set; }
        public string Heading { get; set; }
        public string Value { get; set; }
        public string ClaimText { get; set; }
        public Category Category { get; set; }
        public string ImagePath { get; set; }
    }
}
