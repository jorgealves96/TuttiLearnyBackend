using LearningAppNetCoreApi.Dtos;

namespace LearningAppNetCoreApi.Services
{
    public interface IProfileService
    {
        Task<ProfileStatsDto> GetUserStatsAsync(string firebaseUid);
    }
}
