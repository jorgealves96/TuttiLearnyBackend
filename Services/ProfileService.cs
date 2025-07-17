using LearningAppNetCoreApi.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LearningAppNetCoreApi.Services
{
    public class ProfileService : IProfileService
    {
        private readonly ApplicationDbContext _context;

        public ProfileService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ProfileStatsDto> GetUserStatsAsync(string userAuth0Id)
        {
            var user = await _context.Users
                .Include(u => u.LearningPaths)
                    .ThenInclude(lp => lp.PathItems)
                    .ThenInclude(pi => pi.Resources)
                .FirstOrDefaultAsync(u => u.FirebaseUid == userAuth0Id);

            if (user == null)
            {
                return null; // Or throw a NotFoundException
            }

            var allPaths = user.LearningPaths;
            var allResources = allPaths.SelectMany(p => p.PathItems.SelectMany(pi => pi.Resources)).ToList();

            var completedPaths = allPaths.Where(p =>
                p.PathItems.Any() && p.PathItems.SelectMany(pi => pi.Resources).All(r => r.IsCompleted)
            ).Count();

            var stats = new ProfileStatsDto
            {
                PathsStarted = allPaths.Count,
                PathsCompleted = completedPaths,
                ItemsCompleted = allResources.Count(r => r.IsCompleted),
                JoinedDate = user.CreatedAt
            };

            return stats;
        }
    }
}
