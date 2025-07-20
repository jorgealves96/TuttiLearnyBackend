namespace LearningAppNetCoreApi.Models
{
    public class UserResourceProgress
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int ResourceTemplateId { get; set; }
        public ResourceTemplate ResourceTemplate { get; set; }
        public bool IsCompleted { get; set; } = false;
    }
}
