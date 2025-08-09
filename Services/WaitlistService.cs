using LearningAppNetCoreApi.Dtos;
using LearningAppNetCoreApi.Models;
using Microsoft.EntityFrameworkCore;

// TODO: Remove after app is not on waitlist anymore

namespace LearningAppNetCoreApi.Services
{
    public interface IWaitlistService
    {
        Task AddEmailAsync(WaitlistRequestDto request);
    }

    public class WaitlistService : IWaitlistService
    {
        private readonly ApplicationDbContext _context;

        public WaitlistService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddEmailAsync(WaitlistRequestDto request)
        {
            // Check if the email already exists to prevent duplicates
            var emailExists = await _context.WaitlistEntries
                .AnyAsync(w => w.Email.ToLower() == request.Email.ToLower());

            if (emailExists)
            {
                // You can choose to throw an exception or just return successfully
                // to not let the user know if an email is already on the list.
                // For a waitlist, just returning is often fine.
                return;
            }

            var entry = new WaitlistEntry
            {
                Email = request.Email,
                // Join the list of platforms into a single string for storage
                Platforms = string.Join(",", request.Platforms)
            };

            _context.WaitlistEntries.Add(entry);
            await _context.SaveChangesAsync();
        }
    }
}
