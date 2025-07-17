using LearningAppNetCoreApi.Models;
using System.Security.Claims;

namespace LearningAppNetCoreApi.Services
{
    public interface IUserService
    {
        Task<User> SyncUserAsync(ClaimsPrincipal userPrincipal);
    }
}
