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
        Task<bool> DeleteUserAsync(string firebaseUid);
    }
}
