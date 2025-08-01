using LearningAppNetCoreApi.Models;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace LearningAppNetCoreApi.Services.Jobs
{
    [DisallowConcurrentExecution] // Prevents the job from running multiple times if one takes too long
    public class SubscriptionValidationJob : IJob
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SubscriptionValidationJob> _logger;

        public SubscriptionValidationJob(ApplicationDbContext context, ILogger<SubscriptionValidationJob> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Starting daily subscription validation job...");

            var now = DateTime.UtcNow;

            // Find all non-free users whose subscription has expired
            var expiredUsers = await _context.Users
                .Where(u => u.Tier != SubscriptionTier.Free && u.SubscriptionExpiryDate <= now)
                .ToListAsync();

            if (!expiredUsers.Any())
            {
                _logger.LogInformation("No expired subscriptions found.");
                return;
            }

            foreach (var user in expiredUsers)
            {
                _logger.LogInformation($"Downgrading user {user.Id} from {user.Tier} to Free.");
                user.Tier = SubscriptionTier.Free;
                user.SubscriptionExpiryDate = null;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Successfully processed {expiredUsers.Count} expired subscriptions.");
        }
    }
}