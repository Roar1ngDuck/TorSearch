using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorSearch.Scraper
{
    internal class Data
    {
        /// <summary>
        /// Save the given data to the database using the local web API
        /// </summary>
        /// <param name="url">Url of scraped site</param>
        /// <param name="title">Title of scraped site</param>
        /// <param name="html">Html of scraped site</param>
        public static void SaveData(string url, string title, string html, string description, string keywords, string baseurl)
        {
            var info = new DomainInfo()
            {
                Date = DateTime.Now,
                Html = html,
                Title = title,
                Url = url,
                Baseurl = baseurl,
                Description = description,
                Keywords = keywords,
            };

            var res = HTTP.PostJson("https://127.0.0.1:7156/SaveToDatabase", info);
        }
    }
}
