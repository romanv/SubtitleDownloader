namespace RV.SubD.Tests
{
    using RV.SubD.Core;

    using Xunit;

    public class TitleParserTests
    {
        [Theory]
        [InlineData(true, "Modern.Family.S08E10.720p.HDTV.x264-AVS")]
        [InlineData(7, "adventure.time.s07e38-e39.720p.hdtv.x264-w4f")]
        [InlineData(false, "broken title")]
        public void TitleValidationIsCorrect(bool expected, string input)
        {
            TitleParser.TryParse(out var sut, input);

            Assert.Equal(expected, sut.IsValid);
        }

        [Theory]
        [InlineData(8, "Modern.Family.S08E10.720p.HDTV.x264-AVS")]
        [InlineData(7, "adventure.time.s07e38-e39.720p.hdtv.x264-w4f")]
        [InlineData(1, "S01E01 - Time Slime")]
        [InlineData(2, "SU 2x10 Chille Tid(AsetKeyZet)")]
        [InlineData(-1, "broken title")]
        public void SeasonNumberIsCorrect(int expected, string input)
        {
            var parsedCorrectly = TitleParser.TryParse(out var sut, input);

            Assert.True(parsedCorrectly);
            Assert.Equal(expected, sut.Season);
        }

        [Theory]
        [InlineData(new[] { 10 }, "d:\\Modern.Family.S08E10.720p.HDTV.x264-AVS.mkv")]
        [InlineData(new[] { 38, 39 }, "d:\\adventure.time.s07e38-e39.720p.hdtv.x264-w4f.mkv")]
        [InlineData(new[] { 6 }, "d:\\BoJack.Horsema S03E06.avi")]
        [InlineData(new[] { 1 }, "d:\\S01E01 - Time Slime.mp4")]
        [InlineData(new[] { 10 }, "d:\\SU 2x10 Chille Tid(AsetKeyZet)")]
        [InlineData(new[] { -1 }, "broken title")]
        public void EpisodeNumbersAreCorrect(int[] expected, string input)
        {
            var parsedCorrectly = TitleParser.TryParse(out var sut, input);

            Assert.True(parsedCorrectly);
            Assert.Equal(expected, sut.Episodes);
        }

        [Theory]
        [InlineData("Modern Family", "c:\\Modern.Family.S08E10.720p.HDTV.x264-AVS.avi")]
        [InlineData("adventure time", "z:\\adventure.time.s07e38-e39.720p.hdtv.x264-w4f.mkv")]
        [InlineData("BoJack Horsema", "f:\\BoJack.Horsema S03E06.mp4")]
        [InlineData("", "e:\\S01E01 - Time Slime.avi")]
        [InlineData("SU", "d:\\SU 2x10 Chille Tid(AsetKeyZet).mkv")]
        [InlineData("broken title", "broken title")]
        public void TitleIsCorrect(string expected, string input)
        {
            var parsedCorrectly = TitleParser.TryParse(out var sut, input);

            Assert.True(parsedCorrectly);
            Assert.Equal(expected, sut.Title);
        }
    }
}
