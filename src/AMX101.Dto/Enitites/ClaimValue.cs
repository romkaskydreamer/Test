namespace AMX101.Dto.Enitites
{
    public class ClaimValue
    {
        public int Id { get; set; }
        public int ClaimId { get; set; }
        public string Postcode { get; set; }
        public long? Value { get; set; }
        public int Order { get; set; }
        public int Threshold { get; set; }
    }
}
