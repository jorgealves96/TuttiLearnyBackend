using LearningAppNetCoreApi.Models;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace LearningAppNetCoreApi.Services.Jobs
{
    public class SubscriptionValidationJob
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SubscriptionValidationJob> _logger;

        public SubscriptionValidationJob(ApplicationDbContext context, ILogger<SubscriptionValidationJob> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Changed from Execute to a public method we can call
        public async Task<string> ExecuteAsync()
        {
            _logger.LogInformation("Executing subscription validation logic...");

            var now = DateTime.UtcNow;
            var expiredUsers = await _context.Users
                .Where(u => u.Tier != SubscriptionTier.Free && u.SubscriptionExpiryDate <= now)
                .ToListAsync();

            if (!expiredUsers.Any())
            {
                _logger.LogInformation("No expired subscriptions found.");
                return "No expired subscriptions found.";
            }

            foreach (var user in expiredUsers)
            {
                _logger.LogInformation($"Downgrading user {user.Id} from {user.Tier} to Free.");
                user.Tier = SubscriptionTier.Free;
                user.SubscriptionExpiryDate = null;
            }

            await _context.SaveChangesAsync();
            var successMessage = $"Successfully processed {expiredUsers.Count} expired subscriptions.";
            _logger.LogInformation(successMessage);
            return successMessage;
        }
    }
}