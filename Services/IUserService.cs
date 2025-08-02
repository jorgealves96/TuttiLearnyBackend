using LearningAppNetCoreApi.Dtos;
using LearningAppNetCoreApi.Models;
using System.Security.Claims;

namespace LearningAppNetCoreApi.Services
{
    public interface IUserService
    {
        Task<User> SyncUserAsync(ClaimsPrincipal userPrincipal);
        Task<User?> GetUserByFirebaseUidAsync(string firebaseUid);
        Task<UserSubscriptionStatusDto?> GetUserSubscriptionStatusAsync(string firebaseUid);
        Task<User> UpdateUserNameAsync(string firebaseUid, string newName);
        Task UpdateFcmTokenAsync(string firebaseUid, string fcmToken);
        Task<bool> UpdateNotificationPreferenceAsync(string firebaseUid, bool isEnabled);
        Task<bool> DeleteUserAsync(string firebaseUid);
    }
}
