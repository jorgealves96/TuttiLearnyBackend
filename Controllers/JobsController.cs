using Google.Cloud.SecretManager.V1;
using LearningAppNetCoreApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LearningAppNetCoreApi.Controllers
{
    [ApiController]
    [Route("api/internal/jobs")]
    public class JobsController : ControllerBase
    {
        private readonly IJobsService _jobsService;
        private readonly IConfiguration _configuration;

        public JobsController(IJobsService jobsService, IConfiguration configuration)
        {
            _jobsService = jobsService;
            _configuration = configuration;
        }

        [HttpPost("validate-subscriptions")]
        public async Task<IActionResult> ValidateSubscriptions([FromHeader(Name = "X-Scheduler-Secret")] string secretFromHeader)
        {
            var isAuthorized = await IsAuthorized(secretFromHeader);
            if (!isAuthorized)
            {
                return Unauthorized("Invalid secret.");
            }

            var resultMessage = await _jobsService.RunSubscriptionValidationJobAsync();
            return Ok(resultMessage);
        }

        [HttpPost("reset-monthly-usage")]
        public async Task<IActionResult> ResetMonthlyUsage([FromHeader(Name = "X-Scheduler-Secret")] string secretFromHeader)
        {
            var isAuthorized = await IsAuthorized(secretFromHeader);
            if (!isAuthorized)
            {
                return Unauthorized("Invalid secret.");
            }

            var resultMessage = await _jobsService.RunResetMonthlyUsageJobAsync();
            return Ok(resultMessage);
        }

        [HttpPost("send-learning-reminders")]
        public async Task<IActionResult> SendLearningReminders([FromHeader(Name = "X-Scheduler-Secret")] string secretFromHeader)
        {
            var isAuthorized = await IsAuthorized(secretFromHeader);
            if (!isAuthorized)
            {
                return Unauthorized("Invalid secret.");
            }

            var resultMessage = await _jobsService.RunSendRemindersJobAsync();
            return Ok(resultMessage);
        }

        private async Task<bool> IsAuthorized(string secretFromHeader)
        {
            try
            {
                // 1. Fetch the real secret from Secret Manager on each call
                var projectId = _configuration["Firebase:ProjectId"];
                var secretManager = await SecretManagerServiceClient.CreateAsync();
                var secretVersionName = new SecretVersionName(projectId, "scheduler-job-key", "latest");
                var result = await secretManager.AccessSecretVersionAsync(secretVersionName);
                var expectedSecret = result.Payload.Data.ToStringUtf8();

                // 2. Compare the header key to the real secret
                return !string.IsNullOrEmpty(expectedSecret) && secretFromHeader == expectedSecret;
            }
            catch (Exception)
            {
                // If fetching the secret fails for any reason, deny access.
                return false;
            }
        }
    }
}