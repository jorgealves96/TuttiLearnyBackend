using Microsoft.EntityFrameworkCore;

namespace LearningAppNetCoreApi.Services.Jobs
{
    public class ResetMonthlyUsageJob
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ResetMonthlyUsageJob> _logger;

        public ResetMonthlyUsageJob(ApplicationDbContext context, ILogger<ResetMonthlyUsageJob> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> ExecuteAsync()
        {
            _logger.LogInformation("Executing monthly usage reset job for all users...");

            // This single command updates all rows in the Users table.
            // It's much more efficient than fetching every user and updating them individually.
            var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                "UPDATE \"users\" SET \"paths_generated_this_month\" = 0, \"paths_extended_this_month\" = 0, \"quizzes_created_this_month\" = 0"
            );

            // We don't need to update 'LastUsageResetDate' anymore because the job's
            // execution time itself serves as the record of when the reset happened.

            var successMessage = $"Monthly usage counters reset successfully for {rowsAffected} users at {DateTime.UtcNow}.";
            _logger.LogInformation(successMessage);
            return successMessage;
        }
    }
}