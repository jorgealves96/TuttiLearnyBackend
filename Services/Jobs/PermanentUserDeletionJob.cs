using FirebaseAdmin.Auth;
using Microsoft.EntityFrameworkCore;

namespace LearningAppNetCoreApi.Services.Jobs
{
    public class PermanentUserDeletionJob
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PermanentUserDeletionJob> _logger;

        public PermanentUserDeletionJob(ApplicationDbContext context, ILogger<PermanentUserDeletionJob> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> ExecuteAsync()
        {
            _logger.LogInformation("Starting daily job to permanently delete old accounts...");

            var deletionThreshold = DateTime.UtcNow.AddDays(-30);

            // Find all users who were soft-deleted more than 30 days ago
            var usersToDelete = await _context.Users
                .Where(u => u.IsDeleted && u.DeletedAt <= deletionThreshold)
                .ToListAsync();

            if (usersToDelete.Count == 0)
            {
                _logger.LogInformation("No accounts found for permanent deletion today.");
                return "No accounts found for permanent deletion.";
            }

            int successCount = 0;
            foreach (var user in usersToDelete)
            {
                try
                {
                    // Important: Delete all related data first to avoid foreign key errors
                    var userPaths = _context.UserPaths.Where(up => up.UserId == user.Id);
                    _context.UserPaths.RemoveRange(userPaths);

                    var userProgress = _context.UserResourceProgress.Where(urp => urp.UserId == user.Id);
                    _context.UserResourceProgress.RemoveRange(userProgress);

                    // Add RemoveRange for any other related user data (ratings, reports, etc.)

                    // Now remove the user from your database
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();

                    // Finally, delete the user from Firebase Authentication
                    await FirebaseAuth.DefaultInstance.DeleteUserAsync(user.FirebaseUid);

                    _logger.LogInformation("Permanently deleted user {FirebaseUid}", user.FirebaseUid);
                    successCount++;
                }
                catch (FirebaseAuthException ex) when (ex.AuthErrorCode == AuthErrorCode.UserNotFound)
                {
                    _logger.LogWarning("User {FirebaseUid} was already deleted from Firebase. Cleaned up local data.", user.FirebaseUid);
                    successCount++; // Still count as a success
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to permanently delete user {FirebaseUid}", user.FirebaseUid);
                }
            }

            var resultMessage = $"Successfully and permanently deleted {successCount} out of {usersToDelete.Count} user accounts.";
            _logger.LogInformation(resultMessage);
            return resultMessage;
        }
    }
}