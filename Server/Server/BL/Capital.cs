namespace Server.BL
{
    public class Capital
    {
        public int Id { get; set; }
        public int CountryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
