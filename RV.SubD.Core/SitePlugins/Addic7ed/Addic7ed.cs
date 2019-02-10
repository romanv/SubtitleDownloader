using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("RV.SubD.Tests")]

namespace RV.SubD.Core.SitePlugins.Addic7ed
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using RV.SubD.Core.Data;

    public class Addic7ed : ISearchPlugin
    {
        public string PluginTitle => "addic7ed.com";

        private readonly IPageDownloader _pageDownloader = new Addic7edPageDownloader();
        private readonly ISubtitleListParser _subtitleListParser = new Addic7EdSubtitleListParser();

        public async Task<IList<DownloadableSubtitle>> GetSubtitlesListAsync(
            TitleObject to,
            IList<string> languages,
            CancellationToken cancelToken)
        {
            var pageResponse = await _pageDownloader.SearchAndGetSubtitlesListPageAsync(to, cancelToken);
            var downloadableResults = _subtitleListParser.GetSubtitlesListFromHtml(
                pageResponse.Item1,
                pageResponse.Item2,
                languages,
                to.OriginalFilePath);
            return downloadableResults;
        }
    }
}
