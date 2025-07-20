namespace LearningAppNetCoreApi.Models
{
    public class PathItemTemplate
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Order { get; set; }
        public int PathTemplateId { get; set; }
        public PathTemplate PathTemplate { get; set; }
        public ICollection<ResourceTemplate> Resources { get; set; }
    }
}
