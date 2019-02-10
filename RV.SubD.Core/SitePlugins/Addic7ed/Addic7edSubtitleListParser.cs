namespace RV.SubD.Core.SitePlugins.Addic7ed
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using HtmlAgilityPack;

    using RV.SubD.Core.Data;

    internal class Addic7EdSubtitleListParser : ISubtitleListParser
    {
        private const string PluginTitle = "addic7ed.com";

        private const string Host = "www.addic7ed.com";

        private const string SubtitleBlockDivId = "container95m";
        private const string VersionBlockClass = "NewsTitle";
        private const string LanguageBlockClass = "language";
        private const string HearingImpairedBlockTitle = "Hearing Impaired";
        private const string DownloadsBlockClass = "newsDate";

        public IList<DownloadableSubtitle> GetSubtitlesListFromHtml(
            Uri referrerUri,
            string html,
            IList<string> languages,
            string fileName)
        {
            if (string.IsNullOrEmpty(html))
            {
                return new List<DownloadableSubtitle>();
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var subtitles = new List<DownloadableSubtitle>();
            var siteEpisodeTitle = GetSiteEpisodeTitle(html);

            foreach (var sbBlockDiv in htmlDoc.DocumentNode.SelectNodes($"//div[@id='{SubtitleBlockDivId}']"))
            {
                var isSub = IsSubtitleNode(sbBlockDiv);

                if (!isSub)
                {
                    continue;
                }

                // do nothing if lanuage is not supported
                var language = ParseLanguage(sbBlockDiv);

                if (!languages.Contains(language.ToLower()))
                {
                    continue;
                }

                var version = ParseVersion(sbBlockDiv);

                var hearingImpaired = ParseHearingImpaired(sbBlockDiv);
                var downloads = ParseDownloadsCount(sbBlockDiv);
                var downloadUrl = ParseDownloadUrl(sbBlockDiv, languages);
                var publisher = ParsePublisher(sbBlockDiv);
                var submittedOn = ParseSubmittedDate(sbBlockDiv);

                subtitles.Add(
                    new DownloadableSubtitle(
                        siteEpisodeTitle,
                        version,
                        language,
                        hearingImpaired,
                        downloads,
                        downloadUrl,
                        publisher,
                        submittedOn,
                        referrerUri,
                        fileName));
            }

            return subtitles;
        }

        private static string GetSiteEpisodeTitle(string html)
        {
            var start = html.IndexOf("class=\"titulo\"", StringComparison.Ordinal) + 16;
            var end = html.IndexOf("<small>", start, StringComparison.Ordinal);
            var dirtyTitle = html.Substring(start, end - start);
            return Regex.Replace(dirtyTitle, @"\t|\n|\r", "").Trim();
        }

        private static bool ParseHearingImpaired(HtmlNode n)
        {
            var node = n.SelectNodes($".//img[@title='{HearingImpairedBlockTitle}']");
            return node != null;
        }

        private static string ParseLanguage(HtmlNode n)
        {
            var language =
                n.Descendants()
                    .First(d => d.Attributes.Contains("class") && d.Attributes["class"].Value == LanguageBlockClass)
                    .InnerText;
            return Regex.Replace(language, @"\t|\n|\r", "").Trim();
        }

        private static string ParseVersion(HtmlNode n)
        {
            // Version FLEET, 0.00 MBs
            var version =
                n.Descendants()
                    .First(d => d.Attributes.Contains("class") && d.Attributes["class"].Value == VersionBlockClass)
                    .InnerText;
            return version.Substring(8, version.IndexOf(',') - 8).Trim();
        }

        private static int ParseDownloadsCount(HtmlNode n)
        {
            var blockContent =
                n.Descendants()
                    .First(
                        d =>
                        d.Attributes.Contains("class") && d.Attributes["class"].Value == DownloadsBlockClass
                        && d.InnerText.Contains("times edited"))
                    .InnerText;

            var blocks = blockContent.Split('·');

            if (blocks.Length != 3)
            {
                return -1;
            }

            var downloadsString = blocks[1].Substring(0, blocks[1].IndexOf("Downloads", StringComparison.Ordinal)).Trim();
            return int.TryParse(downloadsString, out var result) ? result : result;
        }

        private static string ParseDownloadUrl(HtmlNode n, IList<string> languages)
        {
            var downloadButtons = n.SelectNodes(".//a[@class='buttonDownload']");

            // select last (newest) link, where corresponding language in neighbor <td> is one of the supported languages
            var newestSupportedLanguageUrlLink =
                downloadButtons.LastOrDefault(
                    btn =>
                    languages.Contains(
                        btn.ParentNode.ParentNode.SelectSingleNode(".//td[@class='language']").InnerText.Split(' ')[0].Trim()
                        .ToLower()));

            return Host + newestSupportedLanguageUrlLink?.Attributes["href"].Value;
        }

        private string ParsePublisher(HtmlNode n)
        {
            var blockContent = n.Descendants()
                    .First(d => d.Attributes.Contains("href") && d.Attributes["href"].Value.StartsWith("/user/"))
                    .InnerText;

            return $"{blockContent}@{PluginTitle}";
        }

        private DateTime ParseSubmittedDate(HtmlNode n)
        {
            var publishDateText = n.SelectSingleNode(".//table[@class=\"tabel95\"]//table[@class=\"tabel95\"]//tr//td[2]").InnerText;

            var dateRg = new Regex(@"uploaded by .* (?<date>\d+) (?<dateType>\w+)");
            var date = dateRg.Match(publishDateText).Groups["date"].Value;
            var dateType = dateRg.Match(publishDateText).Groups["dateType"].Value;

            if (dateType == "days")
            {
                if (int.TryParse(date, out var daysAgo))
                {
                    return DateTime.Now - TimeSpan.FromDays(daysAgo);
                }
            }
            else if (dateType == "hours")
            {
                if (int.TryParse(date, out var hoursAgo))
                {
                    return DateTime.Now - TimeSpan.FromHours(hoursAgo);
                }
            }

            return DateTime.MinValue;
        }

        private static bool IsSubtitleNode(HtmlNode n)
        {
            var commentNode = n.SelectNodes(".//span[@id='comments']");
            return commentNode == null;
        }
    }
}
