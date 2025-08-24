using LearningAppNetCoreApi.Models;
using System.ComponentModel.DataAnnotations;

namespace LearningAppNetCoreApi.Dtos
{
    public class UpdateUserDto
    {
        [Required]
        [MaxLength(50)]
        public required string NewName { get; set; }
    }

    public class UserSubscriptionStatusDto
    {
        public SubscriptionTier Tier { get; set; }
        public int PathsGeneratedThisMonth { get; set; }
        public int PathsExtendedThisMonth { get; set; }
        public int QuizzesCreatedThisMonth { get; set; }
        public int? PathGenerationLimit { get; set; }
        public int? PathExtensionLimit { get; set; }
        public int? QuizCreationLimit { get; set; }
        public DateTime? SubscriptionExpiryDate { get; set; }
        public int? DaysLeftInSubscription { get; set; }
        public DateTime LastUsageResetDate { get; set; }
        public LearningLevel LearningLevel { get; set; }
    }

    public class UpdateFcmTokenDto
    {
        [Required]
        public required string FcmToken { get; set; }
    }

    public class UpdateNotificationPreferenceDto
    {
        public bool IsEnabled { get; set; }
    }

    public class UpdatePathGenerationSettingsDto
    {
        public LearningLevel? LearningLevel { get; set; }
        public PathLength? PathLength { get; set; }
    }

    public class UserSettingsDto
    {
        public LearningLevel LearningLevel { get; set; }
        public PathLength PathLength { get; set; }
    }
}
