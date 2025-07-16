namespace LearningAppNetCoreApi.Models
{
    public class Resource
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Url { get; set; }
        public ItemType Type { get; set; }
        public bool IsCompleted { get; set; } = false;
        public int PathItemId { get; set; }
        public PathItem PathItem { get; set; }
    }
    // The ItemType enum can live here or in its own file
    public enum ItemType
    {
        Article,
        Video,
        Book,
        Project,
        Documentation
    }
}
