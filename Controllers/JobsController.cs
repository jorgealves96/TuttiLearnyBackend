using Google.Cloud.SecretManager.V1;
using LearningAppNetCoreApi.Services.Jobs;
using Microsoft.AspNetCore.Mvc;

namespace LearningAppNetCoreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        [HttpPost("run-learning-reminder")]
        public async Task<IActionResult> RunLearningReminderJob(
            [FromServices] SendLearningRemindersJob job,
            [FromServices] IConfiguration config, // Inject IConfiguration
            [FromHeader(Name = "X-Job-Auth")] string jobAuthKey)
        {
            // 1. Fetch the real secret from Secret Manager
            var projectId = config["Firebase:ProjectId"]; // Get project ID from config
            var secretManager = await SecretManagerServiceClient.CreateAsync();
            var secretVersionName = new SecretVersionName(projectId, "scheduler-job-key", "latest");
            var result = await secretManager.AccessSecretVersionAsync(secretVersionName);
            var secret = result.Payload.Data.ToStringUtf8();

            // 2. Compare the header key to the real secret
            if (jobAuthKey != secret)
            {
                return Unauthorized();
            }

            await job.Execute(null);
            return Ok("Job executed.");
        }
    }
}
