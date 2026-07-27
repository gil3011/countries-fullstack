using System.ComponentModel.DataAnnotations;
using Server.BL;

namespace Server.DTO
{
    public class UserLoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Outbound representation of a user. Deliberately excludes the password hash so
    /// it is never serialized to clients. Use for any endpoint that returns user data.
    /// </summary>
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsBlocked { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsAllowedToShare { get; set; }

        public static UserDto FromUser(User user) => new()
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            IsBlocked = user.IsBlocked,
            IsAdmin = user.IsAdmin,
            IsAllowedToShare = user.IsAllowedToShare
        };
    }

    public class LanguageRequest
    {
        public string Language { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
    }

    public class ChangePasswordRequest
    {
        public int UserId { get; set; }
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public sealed class GenerateCountryQuizRequest
    {
        public string Cca3 { get; set; } = string.Empty;

        public int QuestionCount { get; set; } = 5;

        public string Difficulty { get; set; } = "Medium";
    }

    public sealed class GenerateRegionQuizRequest
    {
        public string Region { get; set; } = string.Empty;

        public int QuestionCount { get; set; } = 5;

        public string Difficulty { get; set; } = "Medium";
    }

    public sealed class GeneratedQuizDto
    {
        public string Title { get; set; } = string.Empty;

        public List<GeneratedQuestionDto> Questions { get; set; } = new();
    }

    public sealed class GeneratedQuestionDto
    {
        public string Text { get; set; } = string.Empty;

        // OptionA is always the correct answer.
        public string OptionA { get; set; } = string.Empty;

        public string OptionB { get; set; } = string.Empty;

        public string OptionC { get; set; } = string.Empty;

        public string OptionD { get; set; } = string.Empty;
    }

    // Client asks for a personalised country recommendation for a given user.
    public sealed class CountryRecommendationRequest
    {
        public int UserId { get; set; }

        // How many ranked recommendations to return.
        public int Count { get; set; } = 3;
    }

    // A single recommended country returned to the client. Country facts are
    // re-hydrated on the server; Reason/Rank come from Gemini.
    public sealed class CountryRecommendationDto
    {
        public int Id { get; set; }

        public string Cca3 { get; set; } = string.Empty;

        public string CommonName { get; set; } = string.Empty;

        public string Region { get; set; } = string.Empty;

        public string FlagUrl { get; set; } = string.Empty;

        // One-sentence, user-specific explanation written by Gemini.
        public string Reason { get; set; } = string.Empty;

        // 1 = best match.
        public int Rank { get; set; }
    }

    // Raw shape Gemini returns for each pick; validated against the shortlist
    // before being turned into a CountryRecommendationDto.
    public sealed class GeminiRecommendationDto
    {
        public string Cca3 { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;

        public int Rank { get; set; }
    }
}
