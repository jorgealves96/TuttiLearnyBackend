using LearningAppNetCoreApi.Dtos;
using LearningAppNetCoreApi.Models;
using LearningAppNetCoreApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningAppNetCoreApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly ILearningPathService _learningPathService;

        public ReportsController(ILearningPathService learningPathService)
        {
            _learningPathService = learningPathService;
        }

        [HttpGet("status/{pathTemplateId}")]
        public async Task<IActionResult> GetReportStatus(int pathTemplateId)
        {
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (firebaseUid == null) return Unauthorized();

            var report = await _learningPathService.GetUserReportForPathAsync(pathTemplateId, firebaseUid);
            if (report == null)
            {
                return NotFound();
            }
            return Ok(report);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitReport([FromBody] SubmitReportDto dto)
        {
            if (dto.ReportType == ReportType.Other && string.IsNullOrWhiteSpace(dto.Description))
            {
                return BadRequest(new { message = "A description is required when selecting 'Other'." });
            }

            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (firebaseUid == null) return Unauthorized();

            var success = await _learningPathService.CreatePathReportAsync(
                dto.PathTemplateId,
                firebaseUid,
                dto.ReportType,
                dto.Description
            );

            if (!success)
            {
                return NotFound(new { message = "User or Path Template not found." });
            }

            return Ok(new { message = "Report submitted successfully." });
        }

        [HttpPost("{pathTemplateId}/acknowledge")]
        public async Task<IActionResult> AcknowledgeReport(int pathTemplateId)
        {
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (firebaseUid == null) return Unauthorized();

            var success = await _learningPathService.AcknowledgeReportAsync(pathTemplateId, firebaseUid);

            if (!success)
            {
                return NotFound(new { message = "No resolved report found to acknowledge." });
            }

            return Ok(new { message = "Report acknowledged successfully." });
        }
    }
}
