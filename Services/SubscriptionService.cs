using LearningAppNetCoreApi.Models;

namespace LearningAppNetCoreApi.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User> UpdateUserSubscriptionAsync(int userId, SubscriptionTier newTier, bool isYearly)
        {
            var user = await _context.Users.FindAsync(userId) ?? throw new Exception("User not found.");

            if (newTier > user.Tier)
            {
                user.PathsGeneratedThisMonth = 0;
                user.PathsExtendedThisMonth = 0;
                user.LastUsageResetDate = DateTime.UtcNow;
            }

            user.Tier = newTier;

            if (newTier == SubscriptionTier.Free)
            {
                user.SubscriptionExpiryDate = null;
            }
            else
            {
                var now = DateTime.UtcNow;
                user.SubscriptionExpiryDate = isYearly ? now.AddYears(1) : now.AddMonths(1);
            }

            await _context.SaveChangesAsync();
            return user;
        }
    }
}
