namespace LearningAppNetCoreApi.Exceptions
{
    public class ApiQuotaExceededException : Exception
    {
        public ApiQuotaExceededException(string message) : base(message) { }
    }
}
