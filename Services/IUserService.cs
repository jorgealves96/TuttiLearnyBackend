using LearningAppNetCoreApi.Models;
using System.Security.Claims;

namespace LearningAppNetCoreApi.Services
{
    public interface IUserService
    {
        Task<User> SyncUserAsync(ClaimsPrincipal userPrincipal);
        Task<User> UpdateUserNameAsync(string firebaseUid, string newName);
        Task<bool> DeleteUserAsync(string firebaseUid);
    }
}
