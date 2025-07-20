using System.ComponentModel.DataAnnotations;

namespace LearningAppNetCoreApi.Dtos
{
    public class UpdateUserDto
    {
        [Required]
        [MaxLength(50)]
        public required string NewName { get; set; }
    }
}
