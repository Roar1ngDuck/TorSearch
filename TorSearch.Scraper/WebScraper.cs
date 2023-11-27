using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TorSearch.Scraper
{
    internal class WebScraper
    {
        private List<UrlData> ScrapeUrlQueue { get; set; } = new List<UrlData>();

        private object ScrapeUrlQueueLock { get; set; } = new object();

        private List<string> ScrapedUrls { get; set; } = new List<string>();

        private object ScrapedUrlsLock { get; set; } = new object();

        private Dictionary<string, int> SeenUrls { get; set; } = new Dictionary<string, int>();

        private int MaxThreads { get; set; }

        private List<string> BlacklistedTerms { get; set; } = new List<string>();

        public WebScraper(string startUrl, int maxThreads, List<string> blacklistedTerms)
        {
            QueueUrlForScraping(startUrl);

            BlacklistedTerms = blacklistedTerms;

            MaxThreads = maxThreads;
        }

        public void StartScraping()
        {
            while (true)
            {
                while (Threading.GetRunningThreads() >= MaxThreads)
                {
                    Thread.Sleep(500);
                }

                var url = GetNextUrl();

                Console.WriteLine("Queue:" + ScrapeUrlQueue.Count);

                if (string.IsNullOrWhiteSpace(url))
                {
                    Thread.Sleep(1000);

                    continue;
                }

                Threading.IncrementThreads(1);

                new Thread(() => Scrape(url)).Start();
            }
        }

        /// <summary>
        /// Returns the base url of a full url. I.e. https://example.com/pages/page1?id=1 will become https://example.com
        /// </summary>
        /// <param name="url">Url to get base url of</param>
        /// <returns></returns>
        private static string GetBaseUrl(string url)
        {
            var protocol = url.Substring(0, url.IndexOf("//") + 2);

            url = url.Replace(protocol, ""); // Remove protocol

            if (!url.Contains("/")) // Append '/' to end if doesn't already contain
                url += "/";

            if (url.Count(x => x == '.') > 1) // Url contains subdomain
            {
                url = url.Substring(url.IndexOf(".") + 1); // Remove subdomain
            }

            var baseUrl = protocol + url.Substring(0, url.IndexOf("/")); // Combine protocol and url until first '/'

            return baseUrl;
        }

        /// <summary>
        /// Scrape a give url
        /// </summary>
        /// <param name="url">Url to scrape</param>
        private void Scrape(string url)
        {
            try
            {
                var response = HTTP.Get(url, TorInstanceControl.GetNextInstancePort()); // HTTP GET with TOR socks proxy

                if (string.IsNullOrWhiteSpace(response)) // Make sure we received something
                {
                    Threading.IncrementThreads(-1);

                    return;
                }

                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(response);

                var links = ResponseParsing.GetAllLinks(url, htmlDocument);

                var plainText = ResponseParsing.ConvertToPlainText(htmlDocument); // Strip html tags

                var title = ResponseParsing.GetTitle(response);

                var description = ResponseParsing.GetDescription(response);

                var keywords = ResponseParsing.GetKeywords(response);

                if (BlacklistedTerms.Any(badTerm => plainText.ToLower().Contains(badTerm))) // Return if the html contains any of the given blacklisted terms
                {
                    Threading.IncrementThreads(-1);

                    return;
                }

                Data.SaveData(url, title, plainText, description, keywords, GetBaseUrl(url)); // Save to database

                foreach (var link in links)
                {
                    var baseUrl = GetBaseUrl(link);
                    if (baseUrl.EndsWith(".onion")) // We only want to scrape TOR websites
                    {
                        QueueUrlForScraping(link);
                    }
                }
            }
            catch { }
            finally
            {
                Threading.IncrementThreads(-1);
            }
        }

        /// <summary>
        /// Adds a give url to the queue to be scraped
        /// </summary>
        /// <param name="url">Url to add to the queue</param>
        private void QueueUrlForScraping(string url)
        {
            var baseUrl = GetBaseUrl(url);

            if (!SeenUrls.ContainsKey(baseUrl)) // If the base url hasn't been seen yet, we add it to the dictionary
            {
                SeenUrls[baseUrl] = 0;
            }

            int seenCount = SeenUrls[baseUrl];

            SeenUrls[baseUrl] += 1;

            var urlData = new UrlData(url, seenCount);

            lock (ScrapedUrlsLock)
            {
                if (ScrapedUrls.Contains(url)) // We don't want to scrape the same url multiple times
                {
                    return;
                }
            }

            lock (ScrapeUrlQueueLock)
            {
                if (ScrapeUrlQueue.Any(x => x.Url == url)) // We don't want to scrape the same url multiple times
                {
                    return;
                }

                ScrapeUrlQueue.Add(urlData);
            }
        }

        /// <summary>
        /// Retrieves the next url to be scraped from the queue. Urls with the least amount of seen base urls are taken first, as this prefers new sites over previously seen ones.
        /// </summary>
        /// <returns></returns>
        private string GetNextUrl()
        {
            string next = string.Empty;

            lock (ScrapeUrlQueueLock)
            {
                if (ScrapeUrlQueue.Count == 0)
                {
                    return next;
                }

                ScrapeUrlQueue = ScrapeUrlQueue.OrderBy(x => x.BaseUrlCount).ToList();

                if (ScrapeUrlQueue.Count >= 1000000)
                {
                    ScrapeUrlQueue = ScrapeUrlQueue.Take(900000).ToList();
                }

                var item = ScrapeUrlQueue.First();

                next = item.Url;

                ScrapeUrlQueue.Remove(item);
            }

            lock (ScrapedUrlsLock)
            {
                ScrapedUrls.Add(next);
            }

            return next;
        }
    }
}
