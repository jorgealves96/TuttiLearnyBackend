using LearningAppNetCoreApi.Models;

namespace LearningAppNetCoreApi.Services
{
    public interface ISubscriptionService
    {
        Task<User> UpdateUserSubscriptionAsync(int userId, SubscriptionTier newTier, bool isYearly);
        Task UpdateSubscriptionFromWebhookAsync(string firebaseUid, string productId, long expirationTimestampMs);
    }
}
