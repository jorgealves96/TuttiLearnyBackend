using Bogus;
using FirebaseAdmin.Auth;
using LearningAppNetCoreApi.Constants;
using LearningAppNetCoreApi.Dtos;
using LearningAppNetCoreApi.Exceptions;
using LearningAppNetCoreApi.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Security.Claims;

namespace LearningAppNetCoreApi.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(ApplicationDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User> SyncUserAsync(ClaimsPrincipal userPrincipal)
        {
            var firebaseUid = userPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(firebaseUid))
            {
                return null;
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
            if (user != null)
            {
                if (user.IsDeleted)
                {
                    var cooldown = TimeSpan.FromDays(30);
                    if (DateTime.UtcNow - user.DeletedAt < cooldown)
                    {
                        // If they are within the cooldown, throw the specific exception.
                        throw new AccountInCooldownException("This account was recently deleted. Please try again later or restore it.");
                    }
                }

                // If user exists, just update the login date and return.
                user.LastLoginDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return user;
            }

            // If user is null, attempt to create them.
            try
            {
                var userName = userPrincipal.FindFirst("name")?.Value;
                var userEmail = userPrincipal.FindFirst(ClaimTypes.Email)?.Value;
                var phoneNumber = userPrincipal.FindFirst("phone_number")?.Value;

                if (string.IsNullOrEmpty(userName))
                {
                    var faker = new Faker();
                    userName = $"{Capitalize(faker.Hacker.Adjective())} {Capitalize(faker.Lorem.Word())}";
                    var args = new UserRecordArgs { Uid = firebaseUid, DisplayName = userName };
                    await FirebaseAuth.DefaultInstance.UpdateUserAsync(args);
                }

                var newUser = new User
                {
                    FirebaseUid = firebaseUid,
                    Email = userEmail,
                    Name = userName,
                    PhoneNumber = phoneNumber
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();
                _logger.LogInformation("New user created: {FirebaseUid}", firebaseUid);
                return newUser;
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                // This specific error code (23505) means a unique constraint was violated.
                // This handles race conditions: another request created the user in the meantime.
                _logger.LogWarning("Race condition detected for user {FirebaseUid}. Re-fetching user.", firebaseUid);
                return await _context.Users.FirstAsync(u => u.FirebaseUid == firebaseUid);
            }
        }

        public async Task<User?> GetUserByFirebaseUidAsync(string firebaseUid)
        {
            if (string.IsNullOrEmpty(firebaseUid))
            {
                return null;
            }

            return await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
        }

        public async Task<UserSubscriptionStatusDto?> GetUserSubscriptionStatusAsync(string firebaseUid)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
            if (user == null) return null;

            // Check if the monthly usage counters need to be reset
            var now = DateTime.UtcNow;
            if (user.LastUsageResetDate.Month != now.Month || user.LastUsageResetDate.Year != now.Year)
            {
                user.PathsGeneratedThisMonth = 0;
                user.PathsExtendedThisMonth = 0;
                user.LastUsageResetDate = now;
                await _context.SaveChangesAsync();
            }

            var dto = new UserSubscriptionStatusDto
            {
                Tier = user.Tier,
                PathsGeneratedThisMonth = user.PathsGeneratedThisMonth,
                PathsExtendedThisMonth = user.PathsExtendedThisMonth,
                SubscriptionExpiryDate = user.SubscriptionExpiryDate,
                QuizzesCreatedThisMonth = user.QuizzesCreatedThisMonth,
                LastUsageResetDate = user.LastUsageResetDate
            };

            if (user.SubscriptionExpiryDate.HasValue)
            {
                TimeSpan remainingTime = user.SubscriptionExpiryDate.Value - DateTime.UtcNow;
                dto.DaysLeftInSubscription = (int)Math.Ceiling(remainingTime.TotalDays);
            }

            switch (user.Tier)
            {
                case SubscriptionTier.Free:
                    dto.PathGenerationLimit = SubscriptionConstants.FreePathGenerationLimit;
                    dto.PathExtensionLimit = SubscriptionConstants.FreePathExtensionLimit;
                    dto.QuizCreationLimit = SubscriptionConstants.FreeQuizCreationLimit;
                    break;
                case SubscriptionTier.Pro:
                    dto.PathGenerationLimit = SubscriptionConstants.ProPathGenerationLimit;
                    dto.PathExtensionLimit = SubscriptionConstants.ProPathExtensionLimit;
                    dto.QuizCreationLimit = SubscriptionConstants.ProQuizCreationLimit;
                    break;
                case SubscriptionTier.Unlimited:
                    dto.PathGenerationLimit = null; // null means unlimited
                    dto.PathExtensionLimit = null;
                    dto.QuizCreationLimit = null;
                    break;
            }

            return dto;
        }

        public async Task<UserSettingsDto?> GetUserSettingsAsync(string firebaseUid)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);

            if (user == null)
            {
                return null; // User not found
            }

            return new UserSettingsDto
            {
                LearningLevel = user.LearningLevel,
                PathLength = user.PathLength
            };
        }

        public async Task<User> UpdateUserNameAsync(string firebaseUid, string newName)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
            if (user == null)
            {
                return null; // User not found
            }

            user.Name = newName;
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task UpdateFcmTokenAsync(string firebaseUid, string fcmToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
            if (user != null)
            {
                user.FcmToken = fcmToken;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> UpdateNotificationPreferenceAsync(string firebaseUid, bool isEnabled)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
            if (user == null)
            {
                return false;
            }

            user.NotificationsEnabled = isEnabled;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdatePathGenerationSettingsAsync(string firebaseUid, UpdatePathGenerationSettingsDto settings)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);

            if (user == null)
            {
                return false;
            }

            if (settings.LearningLevel.HasValue)
            {
                user.LearningLevel = settings.LearningLevel.Value;
            }

            if (settings.PathLength.HasValue)
            {
                user.PathLength = settings.PathLength.Value;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated path generation settings for user {FirebaseUid}", firebaseUid);

            return true;
        }

        public async Task<bool> SoftDeleteAccountAsync(string firebaseUid)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);

            if (user == null || user.IsDeleted)
            {
                // If the user doesn't exist or is already marked as deleted,
                // there's nothing to do. Return false to indicate no change was made.
                return false;
            }

            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {FirebaseUid} has been soft-deleted.", firebaseUid);

            // Return true to signal that the operation was successful.
            return true;
        }

        public async Task<bool> RestoreUserAsync(string firebaseUid)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
            if (user != null && user.IsDeleted)
            {
                user.IsDeleted = false;
                user.DeletedAt = null;
                await _context.SaveChangesAsync();
                _logger.LogInformation("User {FirebaseUid} has been restored.", firebaseUid);
                return true;
            }
            return false;
        }

        #region Private methods

        private static string Capitalize(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            return char.ToUpper(input[0]) + input.Substring(1);
        }

        #endregion
    }
}