using Server.DAL;

namespace Server.BL
{
    public class Country
    {
        public int Id { get; set; }
        public string Cca3 { get; set; } = string.Empty;
        public string CommonName { get; set; } = string.Empty;
        public string OfficialName { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string Subregion { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }        
        public double AreaKm2 { get; set; }
        public bool IsLandlocked { get; set; }
        public int Population { get; set; }

        // ── Flag & media ─────────────────────────────────────

        public string FlagUrl { get; set; } = string.Empty;

        public string WikipediaUrl { get; set; } = string.Empty;


        // ── Navigation / child collections ───────────────────

        public List<Capital> Capitals { get; set; } = new();
        public List<Language> Languages { get; set; } = new();
        public List<Currency> Currencies { get; set; } = new();
        public List<Country> Borders { get; set; } = new();
        public List<string> Timezones { get; set; } = new();


        // --- BLL Methods ---

        public static List<Country> Read()
        {
            DBServiceCountry dbs = new();
            return dbs.ReadAllCountries();
        }

        public static Country GetByCca3(string cca3)
        {
            DBServiceCountry dbs = new();

            if (string.IsNullOrEmpty(cca3))
            {
                throw new ArgumentException("CCA3 code cannot be null or empty.", nameof(cca3));
            }

            return dbs.GetCountriesByCca3(cca3);
        }

        public bool Insert()
        {
            DBServiceCountry dbs = new();
            return dbs.InsertCountry(this);
        }

        public static bool UpdateCountry(int id, Country country)
        {
            DBServiceCountry dbs = new();

            if (country == null)
            {
                throw new ArgumentNullException(nameof(country));
            }

            return dbs.UpdateCountry(id, country);
        }

        public static bool DeleteCountry(int id)
        {
            DBServiceCountry dbs = new();
            return dbs.DeleteCountry(id);
        }
    }
}

