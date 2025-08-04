using System.ComponentModel.DataAnnotations.Schema;

namespace LearningAppNetCoreApi.Models
{
    public class QuizQuestionTemplate
    {
        public int Id { get; set; }
        public string QuestionText { get; set; }

        [Column(TypeName = "jsonb")] // Efficiently store the list of options as JSON
        public List<string> Options { get; set; } = new();

        public int CorrectAnswerIndex { get; set; }
        public int QuizTemplateId { get; set; } // Foreign key
        public QuizTemplate QuizTemplate { get; set; }
    }
}
