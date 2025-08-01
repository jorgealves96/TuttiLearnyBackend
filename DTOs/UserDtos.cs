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
        public int? PathGenerationLimit { get; set; }
        public int? PathExtensionLimit { get; set; }
        public DateTime? SubscriptionExpiryDate { get; set; }
        public int? DaysLeftInSubscription { get; set; }
    }

    public class UpdateFcmTokenDto
    {
        [Required]
        public string FcmToken { get; set; }
    }
}
