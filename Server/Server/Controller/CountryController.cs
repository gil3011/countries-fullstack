using Microsoft.AspNetCore.Mvc;
using Server.BL;
using System.Diagnostics.Metrics;

namespace Server.Conntroller
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountryController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var countries = Country.Read();

                if (countries == null || countries.Count == 0)
                {
                    return NotFound("No countries found.");
                }

                return Ok(countries);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving data.");
            }
        }

        [HttpGet("GetByCca3")]
        public IActionResult Get(string cca3)
        {
            try
            {
                var country = Country.GetByCca3(cca3);
                if (country == null)
                {
                    return NotFound($"Country with code {cca3} not found.");
                }
                return Ok(country);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving data.");
            }

        }

        [HttpPost]
        public IActionResult Post([FromBody] Country country)
        {
            try
            {
                if (country.Insert())
                {
                    return Ok("");
                }
                else
                {
                    return BadRequest("Failed to insert the country into the database.");
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving data.");
            }
        }


        [HttpPut("{id}")]
        public IActionResult UpdateCountry(int id, [FromBody] Country country)
        {
            try
            {
                if (Country.UpdateCountry(id, country))
                {
                    return Ok("");
                }
                else
                {
                    return BadRequest("Failed to insert the country into the database.");
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving data.");
            }
        }

        [HttpDelete("{id}")]
        public IActionResult UpdateCountry(int id)

        {
            try
            {
                if (Country.DeleteCountry(id))
                {
                    return Ok("");
                }
                else
                {
                    return BadRequest("Failed to insert the country into the database.");
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving data.");
            }
        }
        [HttpGet]
        public IActionResult GetLangueges()
        {
            try
            {
                var countries = Country.Read();

                if (countries == null || countries.Count == 0)
                {
                    return NotFound("No countries found.");
                }

                return Ok(countries);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving data.");
            }
        }
    }
}
