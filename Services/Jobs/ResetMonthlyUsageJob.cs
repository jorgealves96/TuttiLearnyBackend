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
            _logger.LogInformation("Executing daily check for monthly usage resets...");

            var oneMonthAgo = DateTime.UtcNow.AddMonths(-1);

            // Find all users whose last reset was more than a month ago
            var usersToReset = await _context.Users
                .Where(u => u.LastUsageResetDate <= oneMonthAgo)
                .ToListAsync();

            if (usersToReset.Count == 0)
            {
                _logger.LogInformation("No users found requiring a usage reset today.");
                return "No users found requiring a usage reset.";
            }

            _logger.LogInformation("Found {UserCount} users to reset.", usersToReset.Count);

            foreach (var user in usersToReset)
            {
                user.PathsGeneratedThisMonth = 0;
                user.PathsExtendedThisMonth = 0;
                user.QuizzesCreatedThisMonth = 0;
                user.LastUsageResetDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            var successMessage = $"Monthly usage counters reset successfully for {usersToReset.Count} users.";
            _logger.LogInformation(successMessage);
            return successMessage;
        }
    }
}