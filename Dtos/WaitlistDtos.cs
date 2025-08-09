using System.ComponentModel.DataAnnotations;

namespace LearningAppNetCoreApi.Dtos
{
    public class WaitlistRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public List<string> Platforms { get; set; }
    }

}
