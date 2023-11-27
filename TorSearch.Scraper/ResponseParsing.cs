using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorSearch.Scraper
{
    internal class ResponseParsing
    {
        /// <summary>
        /// Get all links in a give html document
        /// </summary>
        /// <param name="url">Url to use for relative links</param>
        /// <param name="html">Html document to list links from</param>
        /// <returns></returns>
        public static List<string> GetAllLinks(string url, HtmlDocument html)
        {
            var links = new List<string>();

            try
            {
                if (url.EndsWith("/"))
                {
                    url = url.Substring(0, url.Length - 1);
                }

                foreach (HtmlNode link in html.DocumentNode.SelectNodes("//a[@href]"))
                {
                    // Get the value of the HREF attribute
                    string hrefValue = link.GetAttributeValue("href", string.Empty);

                    if (hrefValue.StartsWith("//"))
                    {
                        hrefValue = url.Substring(0, url.IndexOf("//")) + hrefValue;
                    }
                    else if (!hrefValue.StartsWith("https://") && !hrefValue.StartsWith("http://"))
                    {
                        if (!hrefValue.StartsWith("/"))
                        {
                            hrefValue = "/" + hrefValue;
                        }

                        hrefValue = url + hrefValue;
                    }

                    if (hrefValue.EndsWith("#"))
                    {
                        hrefValue = hrefValue.Substring(0, hrefValue.Length - 1);
                    }

                    links.Add(hrefValue);
                }

            }
            catch { }
            
            return links;
        }

        /// <summary>
        /// Converts HTML to plain text
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <returns></returns>
        public static string ConvertToPlainText(HtmlDocument html)
        {
            StringWriter sw = new StringWriter();
            ConvertTo(html.DocumentNode, sw, 0);
            sw.Flush();
            return sw.ToString();
        }

        private static void ConvertTo(HtmlNode node, TextWriter outText, int count)
        {
            count += 1;

            if (count >= 1000)
            {
                return;
            }

            string html;
            switch (node.NodeType)
            {
                case HtmlNodeType.Comment:
                    // don't output comments
                    break;

                case HtmlNodeType.Document:
                    ConvertContentTo(node, outText, count);
                    break;

                case HtmlNodeType.Text:
                    // script and style must not be output
                    string parentName = node.ParentNode.Name;
                    if ((parentName == "script") || (parentName == "style"))
                        break;

                    // get text
                    html = ((HtmlTextNode)node).Text;

                    // is it in fact a special closing node output as text?
                    if (HtmlNode.IsOverlappedClosingElement(html))
                        break;

                    // check the text is meaningful and not a bunch of whitespaces
                    if (html.Trim().Length > 0)
                    {
                        outText.Write(HtmlEntity.DeEntitize(html));
                    }
                    break;

                case HtmlNodeType.Element:
                    switch (node.Name)
                    {
                        case "p":
                            // treat paragraphs as crlf
                            outText.Write("\r\n");
                            break;
                        case "br":
                            outText.Write("\r\n");
                            break;
                    }

                    if (node.HasChildNodes)
                    {
                        ConvertContentTo(node, outText, count);
                    }
                    break;
            }
        }

        private static void ConvertContentTo(HtmlNode node, TextWriter outText, int count)
        {
            count += 1;

            foreach (HtmlNode subnode in node.ChildNodes)
            {
                ConvertTo(subnode, outText, count);
            }
        }

        /// <summary>
        /// Get description of page from html
        /// </summary>
        /// <param name="html">Html to get description from</param>
        /// <returns></returns>
        public static string GetTitle(string html)
        {
            string toLower = html.ToLower();

            if (!toLower.Contains("<title>") || !toLower.Contains("</title>"))
            {
                return string.Empty;
            }

            int titleStart = toLower.IndexOf("<title>") + 7;
            int titleEnd = toLower.IndexOf("</title>");

            if (titleStart >= titleEnd)
            {
                return string.Empty;
            }

            string title = html.Substring(titleStart, titleEnd - titleStart);

            return title;
        }

        public static string GetDescription(string html)
        {
            string toLower = html.ToLower();

            if (!toLower.Contains("<meta name=\"description\" content=\""))
            {
                return string.Empty;
            }

            int descStart = toLower.IndexOf("<meta name=\"description\" content=\"") + "<meta name=\"description\" content=\"".Length;

            string toLowerTemp = toLower.Substring(descStart);

            if (!toLowerTemp.Contains("\""))
            {
                return string.Empty;
            }

            int descEnd = toLowerTemp.IndexOf("\"") + descStart;

            if (descStart >= descEnd)
            {
                return string.Empty;
            }

            string description = html.Substring(descStart, descEnd - descStart);

            return description;
        }

        public static string GetKeywords(string html)
        {
            string toLower = html.ToLower();

            if (!toLower.Contains("<meta name=\"keywords\" content=\""))
            {
                return string.Empty;
            }

            int descStart = toLower.IndexOf("<meta name=\"keywords\" content=\"") + "<meta name=\"keywords\" content=\"".Length;

            string toLowerTemp = toLower.Substring(descStart);

            if (!toLowerTemp.Contains("\""))
            {
                return string.Empty;
            }

            int descEnd = toLowerTemp.IndexOf("\"") + descStart;

            if (descStart >= descEnd)
            {
                return string.Empty;
            }

            string description = html.Substring(descStart, descEnd - descStart);

            return description;
        }
    }
}
