namespace Server.BL
{
    public class Currency
    {
        public int Id { get; set; }

        /// <summary>ISO 4217 code, e.g. "CAD", "USD"</summary>
        public string CurrencyCode { get; set; } = string.Empty;

        public string CurrencyName { get; set; } = string.Empty;
        public string CurrencySymbol { get; set; } = string.Empty;
    }
}
