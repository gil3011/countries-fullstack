namespace Server.Exceptions
{
    public class GeminiTemporarilyUnavailableException : Exception
    {
        public GeminiTemporarilyUnavailableException() 
            : base("Gemini API is temporarily overloaded or unavailable. All retry attempts failed.")
        {
        }

        public GeminiTemporarilyUnavailableException(string message) 
            : base(message)
        {
        }

        public GeminiTemporarilyUnavailableException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}
