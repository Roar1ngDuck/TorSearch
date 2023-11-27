using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace TorSearch.Scraper
{
    internal class HTTP
    {
        private static List<(HttpClient client, int socksPort)> HttpClients = new List<(HttpClient client, int socksPort)>();

        private static HttpClient PostClient;

        /// <summary>
        /// Perform HTTP GET request using a local socks5 proxy with the specified port
        /// </summary>
        /// <param name="uri">Uri to perform HTTP GET to</param>
        /// <param name="socksPort">Port for local socks5 server</param>
        /// <returns></returns>
        public static string Get(string uri, int socksPort)
        {
            try
            {
                var client = HttpClients.Where(x => x.socksPort == socksPort);

                HttpClient httpClient;

                if (client.Count() == 0 || client == null)
                {
                    var proxy = new WebProxy()
                    {
                        Address = new Uri("socks5://localhost:" + socksPort)
                    };

                    HttpClientHandler handler = new HttpClientHandler()
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator, // Allow connection to sites with invalid certificate
                        Proxy = proxy
                    };
                    httpClient = new HttpClient(handler);
                    httpClient.Timeout = TimeSpan.FromSeconds(60);

                    HttpClients.Add((httpClient, socksPort));
                }
                else
                {
                    httpClient = client.First().client;
                }

                var response = httpClient.GetAsync(uri).Result;

                if (response == null)
                {
                    return "";
                }

                var content = response.Content.ReadAsByteArrayAsync().Result;

                if (content == null)
                {
                    return "";
                }

                return Encoding.UTF8.GetString(content);

            }
            catch { }

            return "";
        }

        /// <summary>
        /// Perform HTTP POST request to the given url and send the specified object in JSON form
        /// </summary>
        /// <param name="url">Url to perform HTTP POST to</param>
        /// <param name="data">Data to send</param>
        /// <returns></returns>
        public static string PostJson(string url, object data)
        {
            try
            {
                if (PostClient == null)
                {
                    HttpClientHandler handler = new HttpClientHandler()
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                    };
                    PostClient = new HttpClient(handler);
                    PostClient.Timeout = TimeSpan.FromSeconds(60);
                }

                var response = PostClient.PostAsJsonAsync(url, data).Result;

                if (response == null)
                {
                    return "";
                }

                var content = response.Content.ReadAsByteArrayAsync().Result;

                if (content == null)
                {
                    return "";
                }

                return Encoding.UTF8.GetString(content);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return "";
        }
    }
}
