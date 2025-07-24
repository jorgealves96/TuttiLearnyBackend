using LearningAppNetCoreApi.Models;

namespace LearningAppNetCoreApi.Dtos
{
    public class UpdateSubscriptionDto
    {
        public SubscriptionTier Tier { get; set; }
        public bool IsYearly { get; set; }
    }
}
