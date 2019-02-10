namespace RV.SubD.Core.SitePlugins.Addic7ed
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
        private const string BaseHost = "http://www.addic7ed.com/";

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

        private async Task<Tuple<Uri, string>> SearchAndGetSubtitlesPageAsync(TitleObject to, CancellationToken cancelToken)
        {
            var searchString = new StringBuilder();

            searchString.Append(BaseHost + "srch.php?search=");
            searchString.AppendFormat(HttpUtility.UrlEncode(to.Title));
            searchString.Append(HttpUtility.UrlEncode($" {to.Season:00}x{to.Episodes.First():00}"));
            searchString.Append("&Submit=Search");

            // trying to avoid usage of system cache
            var handler = new WebRequestHandler { CachePolicy = new HttpRequestCachePolicy(DateTime.MinValue) };

            using (var client = new HttpClient(handler))
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                // trying to avoid usage of system cache
                client.DefaultRequestHeaders.IfModifiedSince = new DateTimeOffset(DateTime.Now);
                client.DefaultRequestHeaders.Add(
                    "User-Agent",
                    "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:43.0) Gecko/20100101 Firefox/43.0");
                var cacheControlHeader = new CacheControlHeaderValue { NoCache = true };
                client.DefaultRequestHeaders.CacheControl = cacheControlHeader;

                using (var response = await client.GetAsync(searchString.ToString(), cancelToken))
                {
                    response.Headers.CacheControl = cacheControlHeader;

                    using (var content = response.Content)
                    {
                        var data = await content.ReadAsStringAsync();

                        if (!string.IsNullOrEmpty(data))
                        {
                            var uri = response.RequestMessage.RequestUri;
                            return new Tuple<Uri, string>(uri, data);
                        }

                        // in case of error
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
                client.DefaultRequestHeaders.Add(
                    "User-Agent",
                    "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:43.0) Gecko/20100101 Firefox/43.0");
                var cacheControlHeader = new CacheControlHeaderValue { NoCache = true };
                client.DefaultRequestHeaders.CacheControl = cacheControlHeader;

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
