using System.Text.Json;
using Google.GenAI;
using Google.GenAI.Types;
using Server.BL;
using Server.DTO;
using Server.Exceptions;
using Microsoft.Extensions.Logging;

using GeminiSchemaType = Google.GenAI.Types.Type;

namespace Server.Services
{
    public sealed class GeminiService
    {
        private readonly Client _client;
        private readonly ILogger<GeminiService> _logger;

        public GeminiService(IConfiguration configuration, ILogger<GeminiService> logger)
        {
            _logger = logger;
            string apiKey = configuration["Gemini:ApiKey"]
                ?? throw new InvalidOperationException(
                    "Gemini API key is missing."
                );

            _client = new Client(apiKey: apiKey);
        }

        public async Task<GeneratedQuizDto> GenerateQuizAsync(
            string topic,
            int questionCount,
            string difficulty,
            List<Country> countries,
            CancellationToken cancellationToken = default)
        {
            ValidateRequest(topic, questionCount, countries);

            string countryData = BuildCountryData(countries);
            string prompt = BuildPrompt(topic, questionCount, difficulty, countryData);
            Schema responseSchema = BuildQuizSchema();

            var response = await GenerateContentWithRetryAsync(prompt, responseSchema, cancellationToken);

            string json = response.Candidates?
                .FirstOrDefault()?
                .Content?
                .Parts?
                .FirstOrDefault()?
                .Text;

            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidOperationException(
                    "Gemini returned an empty response."
                );
            }

            GeneratedQuizDto quiz =
                JsonSerializer.Deserialize<GeneratedQuizDto>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }
                );

            if (quiz == null)
            {
                throw new InvalidOperationException(
                    "Could not convert Gemini response into a quiz."
                );
            }

            ValidateGeneratedQuiz(quiz, questionCount);

            return quiz;
        }

        // ─────────────────────────────────────────────────────────────
        //  Country recommendation (hybrid: C# shortlist + Gemini ranking)
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Recommends countries for a user. The candidate pool and scoring are
        /// computed deterministically in C# (visited countries are excluded here,
        /// never left to the model); Gemini only ranks and explains the shortlist.
        /// </summary>
        public async Task<List<CountryRecommendationDto>> GetRecommendationsAsync(
            List<Country> allCountries,
            List<Country> visited,
            List<Country> wishlist,
            List<string> preferredContinents,
            Dictionary<string, string> languages,
            int count)
        {
            if (count < 1)
            {
                count = 3;
            }

            List<Country> shortlist = BuildShortlist(
                allCountries,
                visited,
                wishlist,
                preferredContinents,
                languages);

            if (shortlist.Count == 0)
            {
                throw new InvalidOperationException(
                    "No countries are available to recommend."
                );
            }

            string prompt = BuildRecommendationPrompt(
                shortlist,
                wishlist,
                preferredContinents,
                languages,
                count);

            Schema responseSchema = BuildRecommendationSchema();

            var response = await GenerateContentWithRetryAsync(prompt, responseSchema, default);

            string json = response.Candidates?
                .FirstOrDefault()?
                .Content?
                .Parts?
                .FirstOrDefault()?
                .Text;

            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidOperationException(
                    "Gemini returned an empty response."
                );
            }

            List<GeminiRecommendationDto> picks =
                JsonSerializer.Deserialize<List<GeminiRecommendationDto>>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }
                );

            if (picks == null || picks.Count == 0)
            {
                throw new InvalidOperationException(
                    "Could not convert Gemini response into recommendations."
                );
            }

            // Only accept picks that are actually in the shortlist we sent
            // (guards against hallucinated country codes), then re-hydrate the
            // full country facts from our own data.
            Dictionary<string, Country> byCca3 = shortlist.ToDictionary(
                c => c.Cca3,
                StringComparer.OrdinalIgnoreCase);

            List<CountryRecommendationDto> recommendations = new();
            HashSet<string> used = new(StringComparer.OrdinalIgnoreCase);

            foreach (GeminiRecommendationDto pick in picks
                .OrderBy(p => p.Rank))
            {
                if (string.IsNullOrWhiteSpace(pick.Cca3))
                {
                    continue;
                }

                if (!byCca3.TryGetValue(pick.Cca3, out Country country))
                {
                    continue;
                }

                if (!used.Add(country.Cca3))
                {
                    continue;
                }

                recommendations.Add(new CountryRecommendationDto
                {
                    Id = country.Id,
                    Cca3 = country.Cca3,
                    CommonName = country.CommonName,
                    Region = country.Region,
                    FlagUrl = country.FlagUrl,
                    Reason = pick.Reason?.Trim() ?? string.Empty,
                    Rank = recommendations.Count + 1
                });

                if (recommendations.Count >= count)
                {
                    break;
                }
            }

            if (recommendations.Count == 0)
            {
                throw new InvalidOperationException(
                    "Gemini did not return any valid recommendations."
                );
            }

            return recommendations;
        }

        /// <summary>
        /// Deterministic candidate selection. Excludes visited and already-wishlisted
        /// countries, scores the rest by continent match, spoken-language match and
        /// affinity with the wishlist, and returns the top slice. Falls back to the
        /// most populous countries when the user has no usable preferences.
        /// </summary>
        private static List<Country> BuildShortlist(
            List<Country> allCountries,
            List<Country> visited,
            List<Country> wishlist,
            List<string> preferredContinents,
            Dictionary<string, string> languages,
            int take = 10)
        {
            HashSet<string> excluded = new(StringComparer.OrdinalIgnoreCase);
            foreach (Country c in visited ?? new())
            {
                if (!string.IsNullOrWhiteSpace(c.Cca3)) excluded.Add(c.Cca3);
            }
            foreach (Country c in wishlist ?? new())
            {
                if (!string.IsNullOrWhiteSpace(c.Cca3)) excluded.Add(c.Cca3);
            }

            HashSet<string> continents = new(
                preferredContinents ?? new(),
                StringComparer.OrdinalIgnoreCase);

            HashSet<string> spokenLanguages = new(
                (languages ?? new()).Keys,
                StringComparer.OrdinalIgnoreCase);

            // Wishlist regions/subregions describe the kind of place the user likes.
            HashSet<string> wishRegions = new(StringComparer.OrdinalIgnoreCase);
            HashSet<string> wishSubregions = new(StringComparer.OrdinalIgnoreCase);
            foreach (Country w in wishlist ?? new())
            {
                if (!string.IsNullOrWhiteSpace(w.Region)) wishRegions.Add(w.Region);
                if (!string.IsNullOrWhiteSpace(w.Subregion)) wishSubregions.Add(w.Subregion);
            }

            var scored = new List<(Country country, int score)>();

            foreach (Country c in allCountries ?? new())
            {
                if (string.IsNullOrWhiteSpace(c.Cca3) || excluded.Contains(c.Cca3))
                {
                    continue;
                }

                int score = 0;

                if (!string.IsNullOrWhiteSpace(c.Region) && continents.Contains(c.Region))
                {
                    score += 3;
                }

                if (c.Languages != null &&
                    c.Languages.Any(l => spokenLanguages.Contains(l.LanguageName)))
                {
                    score += 2;
                }

                if (!string.IsNullOrWhiteSpace(c.Subregion) && wishSubregions.Contains(c.Subregion))
                {
                    score += 2;
                }
                else if (!string.IsNullOrWhiteSpace(c.Region) && wishRegions.Contains(c.Region))
                {
                    score += 1;
                }

                scored.Add((c, score));
            }

            bool anyPreferenceMatch = scored.Any(s => s.score > 0);

            IEnumerable<Country> ordered = anyPreferenceMatch
                ? scored
                    .Where(s => s.score > 0)
                    .OrderByDescending(s => s.score)
                    .ThenByDescending(s => s.country.Population)
                    .Select(s => s.country)
                // No preferences set (or nothing matched): fall back to the most
                // populous, best-known countries so the user still gets a result.
                : scored
                    .OrderByDescending(s => s.country.Population)
                    .Select(s => s.country);

            return ordered.Take(take).ToList();
        }

        private static string BuildRecommendationPrompt(
            List<Country> shortlist,
            List<Country> wishlist,
            List<string> preferredContinents,
            Dictionary<string, string> languages,
            int count)
        {
            string candidates = string.Join(
                System.Environment.NewLine,
                shortlist.Select(c =>
                    $@"Cca3: {c.Cca3}
Country: {c.CommonName}
Region: {c.Region}
Subregion: {c.Subregion}
Languages: {string.Join(", ", (c.Languages ?? new()).Select(l => l.LanguageName))}
---")
            );

            string continentsText = (preferredContinents != null && preferredContinents.Count > 0)
                ? string.Join(", ", preferredContinents)
                : "none specified";

            string languagesText = (languages != null && languages.Count > 0)
                ? string.Join(", ", languages.Select(kv => $"{kv.Key} ({kv.Value})"))
                : "none specified";

            string wishlistText = (wishlist != null && wishlist.Count > 0)
                ? string.Join(", ", wishlist.Select(w => w.CommonName))
                : "none";

            int effectiveCount = Math.Min(count, shortlist.Count);

            return $@"You are a travel recommendation assistant.

Choose the {effectiveCount} best countries for this traveller
from the CANDIDATE COUNTRIES below.

Traveller profile:
- Preferred continents: {continentsText}
- Languages spoken: {languagesText}
- Countries they already want to visit (wishlist): {wishlistText}

Rules:
- Pick ONLY from the candidate list. Use the exact Cca3 codes provided.
- Do not invent countries or codes.
- Rank them from 1 (best match) upward, with no duplicate ranks.
- For each pick, write one short English sentence explaining why it fits
  this traveller, referencing their languages, preferred continents, or the
  style of their wishlist where relevant. Also please add a fact or something insresting about that country
  so the user can be excited and pick it.
- Return exactly {effectiveCount} recommendations.

CANDIDATE COUNTRIES:
{candidates}";
        }

        private static Schema BuildRecommendationSchema()
        {
            Schema itemSchema = new Schema
            {
                Type = GeminiSchemaType.Object,

                Properties = new Dictionary<string, Schema>
                {
                    ["cca3"] = new Schema
                    {
                        Type = GeminiSchemaType.String,
                        Description =
                            "The Cca3 code of a recommended country, " +
                            "copied exactly from the candidate list."
                    },
                    ["reason"] = new Schema
                    {
                        Type = GeminiSchemaType.String,
                        Description =
                            "One short English sentence explaining why this " +
                            "country fits the traveller."
                    },
                    ["rank"] = new Schema
                    {
                        Type = GeminiSchemaType.Integer,
                        Description = "Rank, 1 = best match. No duplicates."
                    }
                },

                Required = new List<string> { "cca3", "reason", "rank" },

                PropertyOrdering = new List<string> { "rank", "cca3", "reason" }
            };

            return new Schema
            {
                Type = GeminiSchemaType.Array,
                Items = itemSchema
            };
        }

        private async Task<GenerateContentResponse> GenerateContentWithRetryAsync(string prompt, Schema responseSchema, CancellationToken cancellationToken)
        {
            var config = new GenerateContentConfig
            {
                ResponseMimeType = "application/json",
                ResponseSchema = responseSchema
            };

            string[] modelsToTry = new[]
            {
                "gemini-3.6-flash",
                "gemini-3.5-flash",
                "gemini-3.5-flash-lite",
                "gemini-3.1-flash-lite",
                "gemini-2.5-flash",
                "gemini-2.5-flash-lite",
                "gemini-2.5-pro"
            };

            int maxAttempts = modelsToTry.Length;
            var random = new Random();

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                string currentModel = modelsToTry[attempt];

                try
                {
                    return await _client.Models.GenerateContentAsync(
                        model: currentModel,
                        contents: prompt,
                        config: config
                    );
                }
                catch (OperationCanceledException)
                {
                    throw; // Do not retry if the request was canceled
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Gemini API call failed on attempt {Attempt} using model '{Model}'. Error: {Message}", attempt + 1, currentModel, ex.Message);

                    if (attempt == maxAttempts - 1)
                    {
                        _logger.LogError(ex, "All {MaxAttempts} Gemini API models failed.", maxAttempts);
                        throw new GeminiTemporarilyUnavailableException($"All {maxAttempts} models failed.", ex);
                    }

                    int waitTimeMs = 1000 + random.Next(0, 500);
                    _logger.LogInformation("Waiting {WaitTimeMs}ms before trying the next model.", waitTimeMs);
                    
                    await Task.Delay(waitTimeMs, cancellationToken);
                }
            }

            throw new GeminiTemporarilyUnavailableException();
        }

        private static void ValidateRequest(
            string topic,
            int questionCount,
            List<Country> countries)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                throw new ArgumentException(
                    "Topic is required."
                );
            }

            if (questionCount < 1 ||
                questionCount > 15)
            {
                throw new ArgumentException(
                    "Question count must be between 1 and 15."
                );
            }

            if (countries.Count < 4)
            {
                throw new ArgumentException(
                    "At least four countries are required to generate a quiz."
                );
            }
        }

        private static string BuildCountryData(
            IEnumerable<Country> countries)
        {
            return string.Join(
                System.Environment.NewLine,
                countries.Select(country =>
                    $@"Country: {country.CommonName}
CCA3: {country.Cca3}
Official name: {country.OfficialName}
Region: {country.Region}
Subregion: {country.Subregion}
Population: {country.Population}
AreaKm2: {country.AreaKm2}
Landlocked: {country.IsLandlocked}
Latitude: {country.Latitude}
Longitude: {country.Longitude}
---"
                )
            );
        }

        private static string BuildPrompt(
            string topic,
            int questionCount,
            string difficulty,
            string countryData)
        {
            return $@"You are a geography quiz generator.

Create exactly {questionCount}
multiple-choice questions about {topic}.

Difficulty: {difficulty}.

Rules:
- Write the quiz title and questions in English.
- Country names in the answers must be copied exactly from the supplied database data.
- OptionA must always be the only correct answer.
- OptionB, OptionC and OptionD must be incorrect.
- Every question must have exactly four distinct options.
- Do not reveal that OptionA is the correct answer.
- Do not create ambiguous or subjective questions.
- Do not repeat questions.
- Use only the supplied database data.
- Do not use outside facts.
- Prefer questions about population, area, subregion, landlocked status and geographic location.
- Make the incorrect answers plausible.

DATABASE DATA:
{countryData}";
        }

        private static Schema BuildQuizSchema()
        {
            Schema questionSchema = new Schema
            {
                Type = GeminiSchemaType.Object,

                Properties = new Dictionary<string, Schema>
                {
                    ["text"] = new Schema
                    {
                        Type = GeminiSchemaType.String,
                        Description = "The quiz question in English."
                    },
                    ["optionA"] = new Schema
                    {
                        Type = GeminiSchemaType.String,
                        Description =
                            "The only correct answer. Must always be correct."
                    },
                    ["optionB"] = new Schema
                    {
                        Type = GeminiSchemaType.String,
                        Description = "A plausible but incorrect answer."
                    },
                    ["optionC"] = new Schema
                    {
                        Type = GeminiSchemaType.String,
                        Description = "A plausible but incorrect answer."
                    },
                    ["optionD"] = new Schema
                    {
                        Type = GeminiSchemaType.String,
                        Description = "A plausible but incorrect answer."
                    }
                },

                Required = new List<string>
                {
                    "text",
                    "optionA",
                    "optionB",
                    "optionC",
                    "optionD"
                },

                PropertyOrdering = new List<string>
                {
                    "text",
                    "optionA",
                    "optionB",
                    "optionC",
                    "optionD"
                }
            };

            return new Schema
            {
                Type = GeminiSchemaType.Object,

                Properties = new Dictionary<string, Schema>
                {
                    ["title"] = new Schema
                    {
                        Type = GeminiSchemaType.String,
                        Description =
                            "A short English title for the quiz."
                    },
                    ["questions"] = new Schema
                    {
                        Type = GeminiSchemaType.Array,
                        Items = questionSchema
                    }
                },

                Required = new List<string>
                {
                    "title",
                    "questions"
                },

                PropertyOrdering = new List<string>
                {
                    "title",
                    "questions"
                }
            };
        }

        private static void ValidateGeneratedQuiz(
            GeneratedQuizDto quiz,
            int expectedQuestionCount)
        {
            if (string.IsNullOrWhiteSpace(quiz.Title))
            {
                throw new InvalidOperationException(
                    "Gemini generated an empty quiz title."
                );
            }

            if (quiz.Questions.Count != expectedQuestionCount)
            {
                throw new InvalidOperationException(
                    $"Expected {expectedQuestionCount} questions, " +
                    $"but Gemini returned {quiz.Questions.Count}."
                );
            }

            for (int i = 0; i < quiz.Questions.Count; i++)
            {
                GeneratedQuestionDto question =
                    quiz.Questions[i];

                if (string.IsNullOrWhiteSpace(question.Text))
                {
                    throw new InvalidOperationException(
                        $"Question {i + 1} has no text."
                    );
                }

                string[] options =
                {
                    question.OptionA,
                    question.OptionB,
                    question.OptionC,
                    question.OptionD
                };

                if (options.Any(string.IsNullOrWhiteSpace))
                {
                    throw new InvalidOperationException(
                        $"Question {i + 1} contains an empty option."
                    );
                }

                int distinctOptionCount = options
                    .Select(option => option.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count();

                if (distinctOptionCount != 4)
                {
                    throw new InvalidOperationException(
                        $"Question {i + 1} contains duplicate options."
                    );
                }
            }
        }
    }
}