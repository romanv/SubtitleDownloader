namespace RV.SubD.Core.SitePlugins.Addic7ed
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Cache;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using HtmlAgilityPack;

    using RV.SubD.Core.Data;
    using RV.SubD.Core.Utils;

    internal class Addic7edPageDownloader : IPageDownloader
    {
        private const string BaseHost = "www.addic7ed.com";

        public async Task<Tuple<Uri, string>> SearchAndGetSubtitlesListPageAsync(TitleObject to, CancellationToken cancelToken)
        {
            var response = await SearchAndGetSubtitlesPageAsync(to, cancelToken);

            if (string.IsNullOrEmpty(response.Item2))
            {
                return new Tuple<Uri, string>(null, string.Empty);
            }

            // check if we got subtitles list page, or page with list of the shows with similar title
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response.Item2);
            if (IsSubtitleListPage(htmlDoc))
            {
                return response;
            }

            // find show link, which title is exactly matches our search
            var links = GetShowLinks(htmlDoc);
            var showLink = GetMatchedLink(to, links);

            if (string.IsNullOrEmpty(showLink))
            {
                return new Tuple<Uri, string>(null, string.Empty);
            }

            var newResponse = await GetSubtitlesPageAsync(showLink);
            return newResponse;
        }

        private async Task<Tuple<bool, Cookie>> GetSessionCookie(CancellationToken cancelToken)
        {
            var handler = new HttpClientHandler();
            var cookieContainer = new CookieContainer();
            handler.CookieContainer = cookieContainer;

            using (var client = new HttpClient(handler))
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; rv:68.0) Gecko/20100101 Firefox/68.0");
                client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
                client.DefaultRequestHeaders.Add("Host", BaseHost);

                var uri = new Uri($"https://{BaseHost}");

                using (var response = await client.GetAsync(uri, cancelToken))
                {
                    var cookies = cookieContainer.GetCookies(uri).Cast<Cookie>().ToList();

                    if (cookies.Any())
                    {
                        var sessionCookie = cookies.FirstOrDefault(c => c.Name == "PHPSESSID");

                        if (sessionCookie != null)
                        {
                            return new Tuple<bool, Cookie>(true, sessionCookie);
                        }
                    }

                    Logger.LogLine($"GetSessionCookie - no session cookie was set by the site. Status is {response.StatusCode}");
                }
            }

            return new Tuple<bool, Cookie>(false, new Cookie());
        }

        private async Task<Tuple<Uri, string>> SearchAndGetSubtitlesPageAsync(TitleObject to, CancellationToken cancelToken)
        {
            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            var (gotSessionCookie, cookie) = await GetSessionCookie(cancelToken);
           
            if (!gotSessionCookie)
            {
                return new Tuple<Uri, string>(new Uri(string.Empty), string.Empty);
            }

            handler.CookieContainer = new CookieContainer();
            handler.CookieContainer.Add(cookie);

            var searchString = new StringBuilder();

            searchString.Append("https://" + BaseHost + "/search.php?search=");
            searchString.AppendFormat(HttpUtility.UrlEncode(to.Title));
            searchString.Append(HttpUtility.UrlEncode($" {to.Season:00}x{to.Episodes.First():00}"));
            searchString.Append("&Submit=Search");

            using (var client = new HttpClient(handler))
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("Host", BaseHost);
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; rv:68.0) Gecko/20100101 Firefox/68.0");
                client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
                client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
                client.DefaultRequestHeaders.Add("Connection", "keep-alive");

                using (var response = await client.GetAsync(searchString.ToString(), cancelToken))
                {
                    using (var content = response.Content)
                    {
                        var data = await content.ReadAsStringAsync();

                        if (!string.IsNullOrEmpty(data))
                        {
                            var uri = response.RequestMessage.RequestUri;
                            return new Tuple<Uri, string>(uri, data);
                        }

                        Logger.LogLine($"SearchAndGetSubtitlesPageAsync - empty data response. Status is {response.StatusCode}");
                    }
                }
            }

            return new Tuple<Uri, string>(new Uri(string.Empty), string.Empty);
        }

        private async Task<Tuple<Uri, string>> GetSubtitlesPageAsync(string url)
        {
            var handler = new WebRequestHandler { CachePolicy = new HttpRequestCachePolicy(DateTime.Now) };

            using (var client = new HttpClient(handler))
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Host = "www.addic7ed.com";
                client.DefaultRequestHeaders.IfModifiedSince = new DateTimeOffset(DateTime.Now);
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; rv:68.0) Gecko/20100101 Firefox/68.0");
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };

                using (var response = await client.GetAsync(BaseHost + url))
                {
                    using (var content = response.Content)
                    {
                        var data = await content.ReadAsStringAsync();

                        if (data == null)
                        {
                            Logger.LogLine("GetSubtitlesPageAsync - data is null");
                            return new Tuple<Uri, string>(null, string.Empty);
                        }

                        var uri = response.RequestMessage.RequestUri;
                        return new Tuple<Uri, string>(uri, data);
                    }
                }
            }
        }

        internal static bool IsSubtitleListPage(HtmlDocument htmlDoc)
        {
            var showTitleNode =
                htmlDoc.DocumentNode.SelectSingleNode(".//span[@class='titulo']");

            return showTitleNode != null;
        }

        internal static IList<string> GetShowLinks(HtmlDocument htmlDoc)
        {
            var showLinks =
                htmlDoc.DocumentNode.Descendants()
                    .Where(d => d.Attributes.Contains("href") && d.Attributes["href"].Value.StartsWith("serie/"))
                    .Select(node => node.GetAttributeValue("href", string.Empty))
                    .ToList();

            return showLinks;
        }

        internal static string GetMatchedLink(TitleObject to, IList<string> links)
        {
            foreach (var link in links)
            {
                var linkShowTitle = link.Substring(6, link.IndexOf("/", 7, StringComparison.Ordinal) - 6);
                if (linkShowTitle == to.Title.Replace(" ", "_"))
                {
                    return link;
                }
            }

            return string.Empty;
        }
    }
}
