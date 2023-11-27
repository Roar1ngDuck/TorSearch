using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorSearch.Scraper
{
    public class DomainInfo
    {
        public string Url { get; set; }
        public string Baseurl { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Keywords { get; set; }
        public string Html { get; set; }
        public DateTime Date { get; set; }
    }
}
