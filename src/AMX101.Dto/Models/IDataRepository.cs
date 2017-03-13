using System.Collections.Generic;
using AMX101.Dto.Enitites;

namespace AMX101.Dto.Models
{

    /// <summary>
    /// A temporary solution before we do the data access refactoring
    /// </summary>

    public interface IDataRepository
    {
        ICollection<Source> GetSources(string region);
        ICollection<ClaimValue> GetClaimValues(string region);
        ICollection<ClaimValue> GetClaimValues(string postcode, string region);
        ICollection<ClaimValue> GetClaimValuesByPrefix(string prefix, string region);
        ICollection<StaticClaim> GetStaticClaims(string region);
        ICollection<StaticClaim> GetStaticClaims(IEnumerable<int> ids, string region);
        ICollection<Claim> GetClaims(string region);
        Claim GetClaim(int id, string region);
        ICollection<Claim> GetClaims(IEnumerable<int> ids, string region);
        ICollection<PostCode> GetPostCodes(string region);
        ICollection<PostCode> SearchPostCodes(string query, string region);
        bool IsValidPostcode(string query, string region);
        void SaveClaims(IEnumerable<Claim> claims);
        void SaveClaimValues(IEnumerable<ClaimValue> claimValues);
        void SaveStaticClaims(IEnumerable<StaticClaim> staticClaims);
        Source GetOrAddSource(string text);
    }
}