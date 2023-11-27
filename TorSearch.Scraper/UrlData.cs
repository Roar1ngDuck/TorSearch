using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorSearch.Scraper
{
    internal class UrlData
    {
        public string Url { get; set; }

        public int BaseUrlCount { get; set; }

        public UrlData(string url, int baseUrlCount)
        {
            Url = url;
            BaseUrlCount = baseUrlCount;
        }
    }
}
