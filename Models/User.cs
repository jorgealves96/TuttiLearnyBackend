﻿namespace LearningAppNetCoreApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FirebaseUid { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public SubscriptionTier Tier { get; set; } = SubscriptionTier.Free;
        public DateTime? SubscriptionExpiryDate { get; set; }

        public int PathsGeneratedThisMonth { get; set; } = 0;
        public int PathsExtendedThisMonth { get; set; } = 0;
        public DateTime LastUsageResetDate { get; set; } = DateTime.UtcNow;
        public int TotalPathsStarted { get; set; } = 0;

        public ICollection<UserPath> UserPaths { get; set; } = new List<UserPath>();
    }

    public enum SubscriptionTier
    {
        Free,
        Pro,      // Tier 1
        Unlimited // Tier 2
    }
}
