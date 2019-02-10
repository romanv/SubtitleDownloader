namespace RV.SubD.Core.Data
{
    using System.Linq;

    public class TitleObject
    {
        public string Title { get; set; }

        public int Season { get; set; }

        public int[] Episodes { get; set; }

        public string OriginalFilePath { get; set; }

        public bool IsValid =>
            !string.IsNullOrEmpty(Title)
            && Season > 0
            && Episodes.Any()
            && !Episodes.Where(e => e <= 0).Any();
    }
}
