namespace Server.DTO
{
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
}