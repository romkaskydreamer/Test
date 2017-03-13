using System.Collections.Generic;
using System.Linq;
using AMX101.Data;
using AMX101.Dto.Models;
using AMX101.Site.Models;

namespace AMX101.Site.Services
{
    public interface IPostcodeService
    {
        IEnumerable<AutocompleteResult> Search(string query, string region);
        bool IsValidPostcode(string query, string region);

    }
    public class PostcodeService : IPostcodeService
    {

        //private readonly IDataRepository _repository;
        //public PostcodeService(IDataRepository repository)
        //{
        //    _repository = repository;
        //}

        public  IEnumerable<AutocompleteResult> Search(string query, string region)
        {
                var lowercaseQuery = query.ToLower();
                var results = new List<AutocompleteResult>();

                var _repository = new DataRepository(region);
                var possiblePostcodes = _repository.SearchPostCodes(query, region);
                results.AddRange(possiblePostcodes.Select(x => new AutocompleteResult()
                {
                    Postcode = x.Postcode.ToString(),
                    State = x.State
                }));

                return results.GroupBy(x => new {x.Postcode, x.State}).Select(x => new AutocompleteResult()
                {
                    Postcode = x.Key.Postcode,
                    State = x.Key.State
                }).Take(5);
        }

        public bool IsValidPostcode(string query, string region)
        {
            var _repository = new DataRepository(region);
            return _repository.IsValidPostcode(query, region);
        }
    }
}
