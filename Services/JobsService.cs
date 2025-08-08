using LearningAppNetCoreApi.Services.Jobs;

namespace LearningAppNetCoreApi.Services
{

    public interface IJobsService
    {
        Task<string> RunSubscriptionValidationJobAsync();
        Task<string> RunResetMonthlyUsageJobAsync();
        Task<string> RunSendRemindersJobAsync();
    }

    public class JobsService : IJobsService
    {
        private readonly SendLearningRemindersJob _sendLearningRemindersJob;
        private readonly SubscriptionValidationJob _subscriptionValidationJob;
        private readonly ResetMonthlyUsageJob _resetMonthlyUsageJob;

        public JobsService(SendLearningRemindersJob _sendLearningRemindersJob,
        SubscriptionValidationJob subscriptionValidationJob,
            ResetMonthlyUsageJob resetMonthlyUsageJob)
        {
            _subscriptionValidationJob = subscriptionValidationJob;
            _resetMonthlyUsageJob = resetMonthlyUsageJob;
        }

        public Task<string> RunSendRemindersJobAsync()
        {
            return _sendLearningRemindersJob.ExecuteAsync();
        }

        public Task<string> RunSubscriptionValidationJobAsync()
        {
            return _subscriptionValidationJob.ExecuteAsync();
        }

        public Task<string> RunResetMonthlyUsageJobAsync()
        {
            return _resetMonthlyUsageJob.ExecuteAsync();
        }
    }
}
