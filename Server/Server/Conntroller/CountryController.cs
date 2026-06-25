using Microsoft.AspNetCore.Mvc;
using Server.BL;

namespace Server.Conntroller
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountryController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<Country> Get()
        {
            return Country.Read();
        }

        [HttpGet("GetByCca3")]
        public Country Get(string cca3)
        {
            return Country.GetByCca3(cca3);
        }

        [HttpPost]
        public bool Post([FromBody] Country country)
        {
            return country.Insert();
        }


        [HttpPut("{id}")]
        public bool UpdateCountry(int id, [FromBody] Country country)
        {
            return Country.UpdateCountry(id, country);
        }

        [HttpDelete("{id}")]
        public bool UpdateCountry(int id)
        {
            return Country.DeleteCountry(id);
        }
    }
}
