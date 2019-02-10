namespace RV.SubD.Tests
{
    using System.IO;

    public class SearchResultsFixture
    {
        public SearchResultsFixture()
        {
            HtmlDummyString = File.ReadAllText("..//..//source.html");
            ShowsListDummyString = File.ReadAllText("..//..//shows_list.htm");
            EmpireBulgarian = File.ReadAllText("..//..//empire_bulgarian.htm");
            TabooDate = File.ReadAllText("..//..//taboo_dates.htm");
        }

        public string HtmlDummyString { get; private set; }

        public string ShowsListDummyString { get; private set; }

        public string EmpireBulgarian { get; set; }

        public string TabooDate { get; set; }
    }
}
