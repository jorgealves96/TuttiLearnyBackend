using LearningAppNetCoreApi.Services.Jobs;

namespace LearningAppNetCoreApi.Services
{
    public class JobsService : IJobsService
    {
        private readonly SendLearningRemindersJob _sendLearningRemindersJob;
        private readonly SubscriptionValidationJob _subscriptionValidationJob;
        private readonly ResetMonthlyUsageJob _resetMonthlyUsageJob;

        public JobsService(SendLearningRemindersJob sendLearningRemindersJob,
        SubscriptionValidationJob subscriptionValidationJob,
            ResetMonthlyUsageJob resetMonthlyUsageJob)
        {
            _subscriptionValidationJob = subscriptionValidationJob;
            _resetMonthlyUsageJob = resetMonthlyUsageJob;
            _sendLearningRemindersJob = sendLearningRemindersJob;
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
