namespace Server.DTO
{
    public sealed class GenerateRegionQuizRequest
    {
        public string Region { get; set; } = string.Empty;

        public int QuestionCount { get; set; } = 5;

        public string Difficulty { get; set; } = "Medium";
    }
}