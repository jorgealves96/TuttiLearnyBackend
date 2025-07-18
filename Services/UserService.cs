using LearningAppNetCoreApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
            // The NameIdentifier claim holds the Firebase UID
            var firebaseUid = userPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(firebaseUid))
            {
                return null;
            }

            // Use the correct property to find the user
            var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);

            if (user == null)
            {
                var userName = userPrincipal.FindFirst("name")?.Value; // 'name' claim from Firebase
                var userEmail = userPrincipal.FindFirst(ClaimTypes.Email)?.Value;

                user = new User
                {
                    FirebaseUid = firebaseUid,
                    Email = userEmail ?? "Not provided",
                    Name = userName ?? "Not provided"
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
    }
}