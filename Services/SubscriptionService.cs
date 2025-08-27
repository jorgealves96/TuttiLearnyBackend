using LearningAppNetCoreApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningAppNetCoreApi.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LearningPathService> _logger;

        public SubscriptionService(ApplicationDbContext context, ILogger<LearningPathService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User> UpdateUserSubscriptionAsync(int userId, SubscriptionTier newTier, bool isYearly)
        {
            var user = await _context.Users.FindAsync(userId) ?? throw new Exception("User not found.");

            if (newTier > user.Tier)
            {
                user.PathsGeneratedThisMonth = 0;
                user.PathsExtendedThisMonth = 0;
                user.QuizzesCreatedThisMonth = 0;
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
            _logger.LogError("User {FirebaseUid} updated his subscription to {Tier}", user.FirebaseUid, newTier);
            return user;
        }

        public async Task UpdateSubscriptionFromWebhookAsync(string firebaseUid, string productId, long expirationTimestampMs)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
            if (user == null)
            {
                _logger.LogWarning("Received webhook for unknown user with Firebase UID: {FirebaseUid}", firebaseUid);
                return; // Exit if the user doesn't exist in our database
            }

            // Map the RevenueCat product ID to your app's SubscriptionTier enum
            var newTier = productId switch
            {
                "pro_monthly" => SubscriptionTier.Pro,
                "pro_yearly" => SubscriptionTier.Pro,
                "unlimited_monthly" => SubscriptionTier.Unlimited,
                "unlimited_yearly" => SubscriptionTier.Unlimited,
                _ => user.Tier // If the product ID is unknown, don't change the tier
            };

            // Convert the expiration timestamp from milliseconds to DateTime
            var newExpiryDate = DateTimeOffset.FromUnixTimeMilliseconds(expirationTimestampMs).UtcDateTime;

            // If the user is upgrading, reset their monthly usage counters
            if (newTier > user.Tier)
            {
                user.PathsGeneratedThisMonth = 0;
                user.PathsExtendedThisMonth = 0;
                user.QuizzesCreatedThisMonth = 0;
                user.LastUsageResetDate = DateTime.UtcNow;
            }

            user.Tier = newTier;
            user.SubscriptionExpiryDate = newExpiryDate;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated subscription for user {FirebaseUid} via webhook. New tier: {Tier}, New Expiry: {ExpiryDate}", firebaseUid, newTier, newExpiryDate);
        }
    }
}
