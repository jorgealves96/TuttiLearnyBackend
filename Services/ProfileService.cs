using LearningAppNetCoreApi.Dtos;
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

        public async Task<ProfileStatsDto> GetUserStatsAsync(string firebaseUid)
        {
            var user = await _context.Users
                .AsNoTracking() // Use AsNoTracking for read-only queries to improve performance
                .FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);

            if (user == null)
            {
                // This is the case for a brand new user who was just synced.
                // Instead of returning null, we return a default DTO with zeroed stats.
                return new ProfileStatsDto
                {
                    PathsStarted = 0,
                    PathsCompleted = 0,
                    ItemsCompleted = 0,
                    JoinedDate = DateTime.UtcNow // Or a default value
                };
            }

            // 1. Get all paths the user has started
            var userPaths = await _context.UserPaths
                .Where(up => up.UserId == user.Id)
                .Include(up => up.PathTemplate)
                    .ThenInclude(pt => pt.PathItems)
                    .ThenInclude(pit => pit.Resources)
                .ToListAsync();

            // 2. Get a set of all completed resource IDs for this user for efficient lookup
            var completedResourceIds = await _context.UserResourceProgress
                .Where(urp => urp.UserId == user.Id && urp.IsCompleted)
                .Select(urp => urp.ResourceTemplateId)
                .ToHashSetAsync();

            // 3. Calculate the number of completed paths
            int completedPathsCount = 0;
            foreach (var userPath in userPaths)
            {
                var allResourceIdsInPath = userPath.PathTemplate.PathItems
                    .SelectMany(pit => pit.Resources)
                    .Select(rt => rt.Id)
                    .ToList();

                // A path is complete if it has resources and all of its resource IDs are in our completed set
                if (allResourceIdsInPath.Any() && allResourceIdsInPath.All(id => completedResourceIds.Contains(id)))
                {
                    completedPathsCount++;
                }
            }

            // 4. Assemble the final statistics DTO
            var stats = new ProfileStatsDto
            {
                PathsStarted = user.TotalPathsStarted,
                PathsInProgress = userPaths.Count,
                PathsCompleted = completedPathsCount,
                ItemsCompleted = completedResourceIds.Count, // The total count of completed resources
                JoinedDate = user.CreatedAt
            };

            return stats;
        }
    }
}
