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
        public IActionResult Get(string cca3)
        {
            var country = Country.GetByCca3(cca3);
            if (country == null)
            {
                return NotFound($"Country with code {cca3} not found.");
            }
            return Ok(country);
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
