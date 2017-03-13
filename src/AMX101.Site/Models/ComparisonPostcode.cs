using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AMX101.Site.Models
{
    public class ComparisonPostcode
    {
        public string Postcode { get; set; }
        public int Cards { get; set; }
        public long Transactions { get; set; }
        public long MerchantSpend { get; set; }
    }
}
