using System.ComponentModel.DataAnnotations;

// TODO: Remove after app is not on waitlist anymore

namespace LearningAppNetCoreApi.Models
{
    public class WaitlistEntry
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; }

        // Store platforms as a simple comma-separated string or as a JSON string
        [Required]
        public string Platforms { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}