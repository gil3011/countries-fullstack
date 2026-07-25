using System.Text.Json;
using Google.GenAI;
using Google.GenAI.Types;
using Server.BL;
using Server.DTO;

using GeminiSchemaType = Google.GenAI.Types.Type;

namespace Server.Services
{
    public sealed class GeminiService
    {
        private readonly Client _client;

        public GeminiService(IConfiguration configuration)
        {
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
            List<Country> countries)
        {
            ValidateRequest(topic, questionCount, countries);

            string countryData = BuildCountryData(countries);
            string prompt = BuildPrompt(topic, questionCount, difficulty, countryData);
            Schema responseSchema = BuildQuizSchema();

            var response = await _client.Models.GenerateContentAsync(
                model: "gemini-3.5-flash",
                contents: prompt,
                config: new GenerateContentConfig
                {
                    ResponseMimeType = "application/json",
                    ResponseSchema = responseSchema
                }
            );

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