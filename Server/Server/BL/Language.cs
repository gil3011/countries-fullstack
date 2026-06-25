namespace Server.BL
{
    public class Language
    {
        public int Id { get; set; }

        /// <summary>ISO 639-1 code, e.g. "en", "fr"</summary>
        public string Iso639_1 { get; set; } = string.Empty;

        public string LanguageName { get; set; } = string.Empty;
    }
}
