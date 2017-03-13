using System.Collections.Generic;
using AMX101.Dto.Models;

namespace AMX101.Site.Models
{
    public class ComparePdfViewModel
    {
        public Industry Industry { get; set; }
        public IEnumerable<ComparisonPostcode> Postcodes { get; set; }
        public int TotalCards { get; set; }
        public long TotalTransactions { get; set; }
        public long TotalMerchantSpend { get; set; }
        public string CardSvg { get; set; }
        public string TransSvg { get; set; }
        public string SpendSvg { get; set; }
        public string ImageDomain { get; set; }
        public IDictionary<int, string> Sources { get; set; }
    }
}
