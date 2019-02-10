namespace RV.SubD.Core.SitePlugins
{
    using System;
    using System.Collections.Generic;

    using RV.SubD.Core.Data;

    internal interface ISubtitleListParser
    {
        IList<DownloadableSubtitle> GetSubtitlesListFromHtml(
            Uri referrerUri,
            string html,
            IList<string> languages,
            string fileName);
    }
}
