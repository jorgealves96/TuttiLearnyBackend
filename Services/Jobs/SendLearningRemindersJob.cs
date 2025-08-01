using FirebaseAdmin.Messaging;
using LearningAppNetCoreApi.Models;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace LearningAppNetCoreApi.Services.Jobs
{
    [DisallowConcurrentExecution]
    public class SendLearningRemindersJob : IJob
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SendLearningRemindersJob> _logger;

        public SendLearningRemindersJob(ApplicationDbContext context, ILogger<SendLearningRemindersJob> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Starting learning reminder job...");

            var inactivityThreshold = DateTime.UtcNow.AddDays(-3);

            // Find users who have an FCM token AND haven't logged in recently
            var inactiveUsers = await _context.Users
                .Where(u => !string.IsNullOrEmpty(u.FcmToken) && u.LastLoginDate < inactivityThreshold)
                .Include(u => u.UserPaths)
                    .ThenInclude(up => up.PathTemplate)
                        .ThenInclude(pt => pt.PathItems)
                            .ThenInclude(pi => pi.Resources)
                .ToListAsync();

            var usersToNotify = new List<User>();

            foreach (var user in inactiveUsers)
            {
                // Check if the user has any path that is not yet complete
                bool hasUnfinishedPath = user.UserPaths.Any(userPath =>
                {
                    var allResourceIdsInPath = userPath.PathTemplate.PathItems
                        .SelectMany(pi => pi.Resources)
                        .Select(r => r.Id)
                        .ToHashSet();

                    if (allResourceIdsInPath.Count == 0) return false; // Path has no content

                    var completedResourceCount = _context.UserResourceProgress
                        .Count(urp => urp.UserId == user.Id &&
                                      allResourceIdsInPath.Contains(urp.ResourceTemplateId) &&
                                      urp.IsCompleted);

                    return completedResourceCount < allResourceIdsInPath.Count;
                });

                if (hasUnfinishedPath)
                {
                    usersToNotify.Add(user);
                }
            }

            foreach (var user in usersToNotify)
            {
                var message = new Message()
                {
                    Notification = new Notification
                    {
                        Title = "Ready to continue your journey?",
                        Body = $"Don't forget to finish your learning path, {user.Name}!"
                    },
                    Token = user.FcmToken
                };

                try
                {
                    await FirebaseMessaging.DefaultInstance.SendAsync(message);
                    _logger.LogInformation($"Sent reminder to user {user.Id}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to send notification to user {user.Id}: {ex.Message}");
                }
            }
            _logger.LogInformation("Learning reminder job finished.");
        }
    }
}
