namespace RV.SubD.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using RV.SubD.Core.SitePlugins.Addic7ed;

    using Xunit;

    public class SearchResultsParserTests : IClassFixture<SearchResultsFixture>
    {
        private readonly string _htmlDummy;
        private readonly string _empireBlgDummy;
        private readonly string _tabooDummy;

        private readonly IList<string> _languages = new List<string> { "english" };

        public SearchResultsParserTests(SearchResultsFixture f)
        {
            _htmlDummy = f.HtmlDummyString;
            _empireBlgDummy = f.EmpireBulgarian;
            _tabooDummy = f.TabooDate;
        }

        [Fact]
        public void VersionsAreCorrect()
        {
            var expected = new List<string>
                               {
                                   "FLEET",
                                   "AVS",
                                   "WEB-DL",
                                   "FLEET",
                                   "AVS",
                                   "WEB-DL"
                               };

            var sut = new Addic7EdSubtitleListParser().GetSubtitlesListFromHtml(
                null,
                _htmlDummy,
                _languages,
                string.Empty);
            var result = sut.Select(s => s.Version);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void UrlsAreCorrect()
        {
            var sut = new Addic7EdSubtitleListParser().GetSubtitlesListFromHtml(
                null,
                _htmlDummy,
                _languages,
                string.Empty);

            Assert.Equal("www.addic7ed.com/original/119605/1", sut[0].DownloadUrl);
            Assert.Equal("www.addic7ed.com/updated/1/119605/0", sut[3].DownloadUrl);
        }

        [Fact]
        public void PublisherIsCorrect()
        {
            var sut = new Addic7EdSubtitleListParser().GetSubtitlesListFromHtml(
                null,
                _htmlDummy,
                _languages,
                string.Empty);

            Assert.Equal("Aaronnmb@addic7ed.com", sut[0].Publisher);
            Assert.Equal("Aaronnmb@addic7ed.com", sut[3].Publisher);
        }

        [Fact]
        public void SiteGlobalTitleIsCorrect()
        {
            var sut = new Addic7EdSubtitleListParser().GetSubtitlesListFromHtml(
                null,
                _htmlDummy,
                _languages,
                string.Empty);

            Assert.Equal("Modern Family - 08x10 - Ringmaster Keifth", sut[0].SiteTitle);
        }

        [Fact]
        public void DownloadUrlForMultilanguageBlockIsCorrect()
        {
            var sut = new Addic7EdSubtitleListParser().GetSubtitlesListFromHtml(
                null,
                _empireBlgDummy,
                _languages,
                string.Empty);

            Assert.Equal(@"www.addic7ed.com/original/118797/1", sut[0].DownloadUrl);
        }

        [Fact]
        public void DatesAreCorrect()
        {
            var sut = new Addic7EdSubtitleListParser().GetSubtitlesListFromHtml(
                null,
                _tabooDummy,
                _languages,
                string.Empty);

            Assert.Equal(new DateTime(2017, 1, 23).Date, sut[0].SubmittedOn.Date);
        }
    }
}
