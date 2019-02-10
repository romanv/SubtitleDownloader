namespace RV.SubD.Core.SitePlugins
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using RV.SubD.Core.Data;

    public interface ISearchPlugin
    {
        Task<IList<DownloadableSubtitle>> GetSubtitlesListAsync(
            TitleObject to,
            IList<string> languages,
            CancellationToken cancelToken);
    }
}