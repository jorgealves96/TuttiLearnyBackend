using LearningAppNetCoreApi.Models;
using System.ComponentModel.DataAnnotations;

namespace LearningAppNetCoreApi.Dtos
{
    public class SubmitReportDto
    {
        public int PathTemplateId { get; set; }
        public ReportType ReportType { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class PathReportDto
    {
        public ReportStatus Status { get; set; }
        public string? ResolutionMessage { get; set; }
    }
}
