using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LearningAppNetCoreApi.Models
{
    [Index(nameof(FirebaseUid), IsUnique = true)]
    public class User
    {
        public int Id { get; set; }

        [Required]
        public required string FirebaseUid { get; set; }

        [EmailAddress]
        [MaxLength(256)]
        public string? Email { get; set; }

        [Phone] 
        [MaxLength(50)]
        public string? PhoneNumber { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public SubscriptionTier Tier { get; set; } = SubscriptionTier.Free;
        public DateTime? SubscriptionExpiryDate { get; set; }

        public int PathsGeneratedThisMonth { get; set; } = 0;
        public int PathsExtendedThisMonth { get; set; } = 0;
        public int QuizzesCreatedThisMonth { get; set; } = 0;
        public DateTime LastUsageResetDate { get; set; } = DateTime.UtcNow;
        public int TotalPathsStarted { get; set; } = 0;

        public ICollection<UserPath> UserPaths { get; set; } = new List<UserPath>();
        public string? FcmToken { get; set; }
        public DateTime LastLoginDate { get; set; } = DateTime.UtcNow;
        public bool NotificationsEnabled { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }

    public enum SubscriptionTier
    {
        Free,
        Pro,      // Tier 1
        Unlimited // Tier 2
    }
}
