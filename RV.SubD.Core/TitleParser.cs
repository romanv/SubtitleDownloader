namespace RV.SubD.Core
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using RV.SubD.Core.Data;

    public static class TitleParser
    {
        public static bool TryParse(out TitleObject result, string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);

            result = new TitleObject
                         {
                             OriginalFilePath = filePath,
                             Title = GetTitle(fileName),
                             Season = GetSeasonNumber(fileName),
                             Episodes = GetEpisodeNumbers(fileName)
                         };
            return true;
        }

        private static string GetTitle(string input)
        {
            // check if input contains season number, which would mean that it is indeed a series,
            // otherwise just return whole string as a title
            var seasonRg = new Regex(@"(?:[Ss](?<season>\d{1,2}))|(?:(?<season>\d{1,2})x\d{1,2})");
            var seasonResult = seasonRg.Match(input).Groups["season"];

            if (!seasonResult.Success)
            {
                return input.Replace('.', ' ').Trim();
            }

            var titleTerminatorRg = new Regex(@"(?:[Ss]\d)|(?:\s\d)|$");
            var titleTerminatorIndex = titleTerminatorRg.Match(input).Index;
            string dirtyTitle = input.Substring(0, titleTerminatorIndex);
            var cleanTitle = dirtyTitle.Replace('.', ' ').Trim();

            return cleanTitle;
        }

        private static int[] GetEpisodeNumbers(string input)
        {
            var episodeRg = new Regex(@"(?:(?:[\d.-][eE])|(?:\dx))(?<episode>\d{1,3})");
            var episodeResults = episodeRg.Matches(input);

            if (episodeResults.Count < 1)
            {
                return new[] { -1 };
            }

            var episodes = new List<int>();

            foreach (Match match in episodeResults)
            {
                foreach (Group grp in match.Groups.Cast<Group>().Skip(1))
                {
                    int episode;

                    if (int.TryParse(grp.Value, out episode))
                    {
                        episodes.Add(episode);
                    }
                }
            }

            return episodes.ToArray();
        }

        private static int GetSeasonNumber(string input)
        {
            var seasonRg = new Regex(@"(?:[Ss](?<season>\d{1,2}))|(?:(?<season>\d{1,2})x\d{1,2})");
            var res = seasonRg.Match(input).Groups["season"];

            int result;
            return int.TryParse(res.Value, out result) ? result : -1;
        }
    }
}
