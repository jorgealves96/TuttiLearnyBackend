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
            var auth0Id = userPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(auth0Id))
            {
                return null;
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == auth0Id);

            if (user == null)
            {
                var userName = userPrincipal.FindFirst(ClaimTypes.Name)?.Value;
                var userEmail = userPrincipal.FindFirst(ClaimTypes.Email)?.Value;

                user = new User
                {
                    FirebaseUid = auth0Id,
                    Email = userEmail ?? "Not provided",
                    Name = userName ?? "Not provided"
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            return user;
        }
    }
}