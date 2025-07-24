namespace LearningAppNetCoreApi.Exceptions
{
    public class UsageLimitExceededException : Exception
    {
        public UsageLimitExceededException(string message) : base(message) { }
    }
}
