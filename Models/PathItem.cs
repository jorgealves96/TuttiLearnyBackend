namespace LearningAppNetCoreApi.Models
{
    public class PathItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Order { get; set; }
        public bool IsCompleted { get; set; } = false;

        public int LearningPathId { get; set; }
        public LearningPath LearningPath { get; set; }

        public ICollection<Resource> Resources { get; set; }
    }
}
