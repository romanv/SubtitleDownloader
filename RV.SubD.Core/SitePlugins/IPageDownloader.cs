namespace RV.SubD.Core.SitePlugins
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using RV.SubD.Core.Data;

    public interface IPageDownloader
    {
        Task<Tuple<Uri, string>> SearchAndGetSubtitlesListPageAsync(TitleObject to, CancellationToken cancelToken);
    }
}