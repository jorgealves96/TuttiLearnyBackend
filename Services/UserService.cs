using LearningAppNetCoreApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Bogus;
using FirebaseAdmin.Auth;

namespace LearningAppNetCoreApi.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User> SyncUserAsync(ClaimsPrincipal userPrincipal)
        {
            var firebaseUid = userPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(firebaseUid))
            {
                return null;
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);

            if (user == null)
            {
                var userName = userPrincipal.FindFirst("name")?.Value;
                var userEmail = userPrincipal.FindFirst(ClaimTypes.Email)?.Value;

                if (string.IsNullOrEmpty(userName))
                {
                    var faker = new Faker();
                    userName = $"{Capitalize(faker.Hacker.Adjective())} {Capitalize(faker.Lorem.Word())}";

                    // This is the missing step
                    var args = new UserRecordArgs
                    {
                        Uid = firebaseUid,
                        DisplayName = userName
                    };
                    await FirebaseAuth.DefaultInstance.UpdateUserAsync(args);
                }

                user = new User
                {
                    FirebaseUid = firebaseUid,
                    Email = userEmail ?? "Not provided",
                    Name = userName
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            return user;
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

        public async Task<bool> DeleteUserAsync(string firebaseUid)
        {
            // Find the user in your local database
            var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);

            if (user != null)
            {
                // If the user has related data (like UserPaths), you must delete that first
                // to avoid foreign key constraint errors.
                var userPaths = await _context.UserPaths
                    .Where(up => up.UserId == user.Id)
                    .ToListAsync();

                if (userPaths.Any())
                {
                    _context.UserPaths.RemoveRange(userPaths);
                }

                // Now remove the user from your database
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }

            try
            {
                // Finally, delete the user from Firebase Authentication
                await FirebaseAuth.DefaultInstance.DeleteUserAsync(firebaseUid);
                return true; // Success
            }
            catch (FirebaseAuthException ex)
            {
                // Handle cases where the user might not exist in Firebase anymore
                // but you still want to clean up your local DB.
                // You can log this error for debugging.
                Console.WriteLine($"Error deleting user from Firebase: {ex.Message}");
                // If the user was not found in Firebase, we can consider it a success
                // because the end goal (user is gone) is achieved.
                if (ex.AuthErrorCode == AuthErrorCode.UserNotFound)
                {
                    return true;
                }
                return false; // An actual error occurred
            }
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