﻿using System.Xml.XPath;

namespace LearningAppNetCoreApi.Models
{
    public class LearningPath
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string GeneratedFromPrompt { get; set; }
        public PathCategory Category { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int UserId { get; set; }
        public User User { get; set; }

        public ICollection<PathItem> PathItems { get; set; }
    }

    public enum PathCategory
    {
        Technology,
        CreativeArts,
        Music,
        Business,
        Wellness,
        LifeSkills,
        Academic,
        Other
    }
}
