namespace LearningAppNetCoreApi.Models
{
    public class ResourceTemplate
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Url { get; set; }
        public ItemType Type { get; set; }
        public int PathItemTemplateId { get; set; }
        public PathItemTemplate PathItemTemplate { get; set; }
    }

    public enum ItemType
    {
        Article,
        Video,
        Book,
        Project,
        Documentation
    }
}
