namespace LearningAppNetCoreApi.Services
{
    public interface IJobsService
    {
        Task<string> RunSubscriptionValidationJobAsync();
        Task<string> RunResetMonthlyUsageJobAsync();
        Task<string> RunSendRemindersJobAsync();
    }
}
