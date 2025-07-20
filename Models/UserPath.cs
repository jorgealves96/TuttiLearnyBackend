using Microsoft.EntityFrameworkCore;

namespace LearningAppNetCoreApi.Models
{
    [Index(nameof(UserId), nameof(PathTemplateId), IsUnique = true)]
    public class UserPath
    {
        // The Id is now the single primary key and will be auto-generated.
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int PathTemplateId { get; set; }
        public PathTemplate PathTemplate { get; set; }
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    }
}
