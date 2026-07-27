using Microsoft.AspNetCore.Mvc;
using Server.Services;
using Server.Exceptions;

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
    public async Task<IActionResult> GenerateRegionQuiz([FromBody] Server.DTO.GenerateRegionQuizRequest request, CancellationToken cancellationToken)
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
                countries: regionCountries,
                cancellationToken: cancellationToken);
            
            return Ok(new
            {
                success = true,
                data = quizDto
            });
        }
        catch (GeminiTemporarilyUnavailableException ex)
        {
            return StatusCode(503, new
            {
                success = false,
                error = "שירות יצירת השאלונים עמוס כרגע. נסה שוב בעוד זמן קצר."
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
    public async Task<IActionResult> GenerateCountryQuiz([FromBody] Server.DTO.GenerateCountryQuizRequest request, CancellationToken cancellationToken)
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
                countries: regionCountries,
                cancellationToken: cancellationToken);
            
            return Ok(new
            {
                success = true,
                data = quizDto
            });
        }
        catch (GeminiTemporarilyUnavailableException ex)
        {
            return StatusCode(503, new
            {
                success = false,
                error = "שירות יצירת השאלונים עמוס כרגע. נסה שוב בעוד זמן קצר."
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

    [HttpPost("recommend")]
    public async Task<IActionResult> RecommendCountries([FromBody] Server.DTO.CountryRecommendationRequest request)
    {
        try
        {
            if (request == null || request.UserId <= 0)
            {
                return BadRequest(new { success = false, error = "A valid userId is required." });
            }

            // Gather everything the recommender needs. Filtering/scoring happen
            // in C# (see GeminiService.BuildShortlist); Gemini only ranks.
            var allCountries = Server.BL.Country.Read();
            var visited = Server.BL.User.getVisited(request.UserId);
            var wishlist = Server.BL.User.getWishlist(request.UserId);
            var continents = Server.BL.User.GetContinentPrefernces(request.UserId);
            var languages = Server.BL.User.GetUserLanguages(request.UserId);

            var recommendations = await _geminiService.GetRecommendationsAsync(
                allCountries: allCountries,
                visited: visited,
                wishlist: wishlist,
                preferredContinents: continents,
                languages: languages,
                count: request.Count);

            return Ok(new
            {
                success = true,
                data = recommendations
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
