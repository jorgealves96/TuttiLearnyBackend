using System.ComponentModel.DataAnnotations;

namespace LearningAppNetCoreApi.DTOs
{
    public class UpdateUserDto
    {
        [Required]
        [MaxLength(50)]
        public required string NewName { get; set; }
    }
}
