using System.ComponentModel.DataAnnotations;

namespace LearningAppNetCoreApi.Dtos
{
    public class SubmitRatingDto
    {
        [Required]
        [Range(1, 10)]
        public int Rating { get; set; }
    }
}
