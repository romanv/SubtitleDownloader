namespace RV.SubD.Tests
{
    using System.Collections.Generic;

    using HtmlAgilityPack;

    using RV.SubD.Core.Data;
    using RV.SubD.Core.SitePlugins.Addic7ed;

    using Xunit;

    public class Addic7edPageDownloaderTests : IClassFixture<SearchResultsFixture>
    {
        private readonly string _htmlDummy;
        private readonly string _showsListHtmlDummy;

        public Addic7edPageDownloaderTests(SearchResultsFixture f)
        {
            _htmlDummy = f.HtmlDummyString;
            _showsListHtmlDummy = f.ShowsListDummyString;
        }

        [Fact]
        public void PageWithListOfSubtitles_IsSubtitlesListPage_True()
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(_htmlDummy);

            Assert.True(Addic7edPageDownloader.IsSubtitleListPage(htmlDoc));
        }

        [Fact]
        public void PageWithListOfShowsLinks_IsSubtitlesListPage_False()
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(_showsListHtmlDummy);

            Assert.False(Addic7edPageDownloader.IsSubtitleListPage(htmlDoc));
        }

        [Fact]
        public void PageWithListOfShowsLinks_ReturnsCorrectLinks()
        {
            var expected = new List<string> { "serie/Vikings/1/1/Rites_of_Passage", "serie/Vikingshill/1/1/Episode_1" };
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(_showsListHtmlDummy);

            Assert.Equal(expected, Addic7edPageDownloader.GetShowLinks(htmlDoc));
        }

        [Fact]
        public void GetMatchedLink_ReturnsCorrectLink()
        {
            var to = new TitleObject { Title = "Vikings" };
            var links = new List<string> { "serie/Vikings/1/1/Rites_of_Passage", "serie/Vikingshill/1/1/Episode_1" };

            Assert.Equal("serie/Vikings/1/1/Rites_of_Passage", Addic7edPageDownloader.GetMatchedLink(to, links));
        }
    }
}
