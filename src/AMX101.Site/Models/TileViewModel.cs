using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AMX101.Site.Models
{
    public class TileViewModel
    {
        public string ImagePath { get; set; }
        public string Heading { get; set; }
        public string Value { get; set; }
        public string FormattedText { get; set; }
        public string ImageDomain { get; set; }
        public int? SourceId { get; set; }
    }
}
