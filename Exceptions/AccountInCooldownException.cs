namespace LearningAppNetCoreApi.Exceptions
{
    public class AccountInCooldownException : Exception
    {
        public AccountInCooldownException(string message) : base(message) { }
    }
}
