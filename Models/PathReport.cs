using System.ComponentModel.DataAnnotations;

namespace LearningAppNetCoreApi.Models
{
    public class PathReport
    {
        public int Id { get; set; }
        public int PathTemplateId { get; set; } // Which path is being reported
        public int UserId { get; set; }         // Who submitted the report

        [Required]
        public ReportType Type { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; } // Optional user description

        public ReportStatus Status { get; set; } = ReportStatus.Submitted;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public User User { get; set; }
        public PathTemplate PathTemplate { get; set; }

        [MaxLength(500)]
        public string? ResolutionMessage { get; set; }
        public bool UserAcknowledged { get; set; } = false;
    }

    public enum ReportType
    {
        InaccurateContent,
        BrokenLinks,
        InappropriateContent,
        Other
    }

    public enum ReportStatus
    {
        Submitted,
        InReview,
        Resolved
    }
}
