using System;
using System.Collections.Generic;
using AMX101.Dto.Enitites;

namespace AMX101.LocalData
{
    public class RegionRepository
    {
        public ICollection<Claim> Claims { get; set; }
        public ICollection<StaticClaim> StaticClaims { get; set; }
        public ICollection<Source> Sources { get; set; }
        public ICollection<PostCode> PostCodes { get; set; }
        public Dictionary<string, ICollection<ClaimValue>> Values { get; set; }

        public ICollection<ClaimValue> GetClaimValues(string postcode)
        {
                if (Values.ContainsKey(postcode))
                {
                    return Values[postcode];
                }
            throw new Exception($"No values for the postcode: {postcode}");
        }
    }
}
