namespace RV.SubD.Core.Data
{
    using System;

    public class DownloadableSubtitle
    {
        public DownloadableSubtitle(
            string siteTitle,
            string version,
            string language,
            bool forHearingImpaired,
            int downloads,
            string downloadUrl,
            string publisher,
            DateTime submittedOn,
            Uri referrerUri,
            string originalFilePath)
        {
            SiteTitle = siteTitle;
            Version = version;
            Language = language;
            ForHearingImpaired = forHearingImpaired;
            Downloads = downloads;
            DownloadUrl = downloadUrl;
            Publisher = publisher;
            SubmittedOn = submittedOn;
            ReferrerUri = referrerUri;
            OriginalFilePath = originalFilePath;
        }

        public string SiteTitle { get; private set; }

        public string Version { get; private set; }

        public string Language { get; private set; }

        public bool ForHearingImpaired { get; private set; }

        public int Downloads { get; private set; }

        public string DownloadUrl { get; private set; }

        public string Publisher { get; private set; }

        public DateTime SubmittedOn { get; private set; }

        public Uri ReferrerUri { get; private set; }

        public string OriginalFilePath { get; private set; }
    }
}
