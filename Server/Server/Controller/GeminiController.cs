using Microsoft.AspNetCore.Mvc;
using Server.Services;

namespace Server.Controllers;

[ApiController]
[Route("api/gemini")]
public class GeminiController : ControllerBase
{
    private readonly GeminiService _geminiService;

    public GeminiController(
        GeminiService geminiService)
    {
        _geminiService = geminiService;
    }

    [HttpPost("generate-region-quiz")]
    public async Task<IActionResult> GenerateRegionQuiz([FromBody] Server.DTO.GenerateRegionQuizRequest request)
    {
        try
        {
            // Fetch all countries and filter by the requested region
            var allCountries = Server.BL.Country.Read();
            var regionCountries = allCountries
                .Where(c => c.Region != null && c.Region.Equals(request.Region, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (regionCountries.Count < 4)
            {
                return BadRequest(new 
                { 
                    success = false, 
                    error = $"Not enough countries found for region '{request.Region}'. Found {regionCountries.Count}, minimum 4 required." 
                });
            }

            // Generate the quiz via Gemini
            var quizDto = await _geminiService.GenerateQuizAsync(
                topic: $"the region: {request.Region}",
                questionCount: request.QuestionCount,
                difficulty: request.Difficulty,
                countries: regionCountries);
            
            return Ok(new
            {
                success = true,
                data = quizDto
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                error = ex.Message
            });
        }
    }

    [HttpPost("generate-country-quiz")]
    public async Task<IActionResult> GenerateCountryQuiz([FromBody] Server.DTO.GenerateCountryQuizRequest request)
    {
        try
        {
            var allCountries = Server.BL.Country.Read();
            var targetCountry = allCountries.FirstOrDefault(c => string.Equals(c.Cca3, request.Cca3, StringComparison.OrdinalIgnoreCase));

            if (targetCountry == null)
            {
                return NotFound(new { success = false, error = $"Country with CCA3 '{request.Cca3}' not found." });
            }

            // Get all countries in the same region to act as distractors
            var regionCountries = allCountries
                .Where(c => c.Region != null && c.Region.Equals(targetCountry.Region, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (regionCountries.Count < 4)
            {
                return BadRequest(new 
                { 
                    success = false, 
                    error = $"Not enough countries found in the same region ('{targetCountry.Region}') to generate distractors. Found {regionCountries.Count}, minimum 4 required." 
                });
            }

            // Generate the quiz via Gemini
            var quizDto = await _geminiService.GenerateQuizAsync(
                topic: $"the country: {targetCountry.CommonName}",
                questionCount: request.QuestionCount,
                difficulty: request.Difficulty,
                countries: regionCountries);
            
            return Ok(new
            {
                success = true,
                data = quizDto
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                error = ex.Message
            });
        }
    }
}
