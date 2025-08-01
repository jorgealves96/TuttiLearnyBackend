using LearningAppNetCoreApi.Dtos;
using LearningAppNetCoreApi.Exceptions;
using LearningAppNetCoreApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningAppNetCoreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PathsController : ControllerBase
    {
        private readonly ILearningPathService _learningPathService;

        public PathsController(ILearningPathService learningPathService)
        {
            _learningPathService = learningPathService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUserPaths()
        {
            // Get the user's unique ID from the token's 'sub' (subject) claim
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(firebaseUid))
            {
                return Unauthorized();
            }

            var paths = await _learningPathService.GetUserPathsAsync(firebaseUid);
            return Ok(paths);
        }

        [Authorize]
        [HttpGet("{userPathId}")]
        public async Task<IActionResult> GetPathById(int userPathId)
        {
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var path = await _learningPathService.GetPathByIdAsync(userPathId, firebaseUid);
            if (path == null) return NotFound();
            return Ok(path);
        }

        [HttpPost("suggestions")]
        public async Task<IActionResult> GetSuggestions([FromBody] CreatePathRequestDto dto)
        {
            var suggestions = await _learningPathService.FindSimilarPathsAsync(dto.Prompt);
            return Ok(suggestions);
        }

        [Authorize]
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateNewPath([FromBody] CreatePathRequestDto dto)
        {
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (firebaseUid == null) return Unauthorized();

            try
            {
                var newPath = await _learningPathService.GenerateNewPathAsync(dto.Prompt, firebaseUid);
                return CreatedAtAction(nameof(GetPathById), new { userPathId = newPath.UserPathId }, newPath);
            }
            catch (UsageLimitExceededException ex)
            {
                // Return 429 Too Many Requests for usage limit errors
                return StatusCode(429, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("templates/{templateId}/assign")]
        public async Task<IActionResult> AssignPath(int templateId)
        {
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(firebaseUid)) return Unauthorized();

            var assignedPath = await _learningPathService.AssignPathToUserAsync(templateId, firebaseUid);
            if (assignedPath == null) return NotFound();
            return CreatedAtAction(nameof(GetPathById), new { userPathId = assignedPath.UserPathId }, assignedPath);
        }

        [Authorize]
        [HttpPost("{userPathId}/extend")]
        public async Task<IActionResult> ExtendLearningPath(int userPathId)
        {
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (firebaseUid == null) return Unauthorized();

            try
            {
                var newItems = await _learningPathService.ExtendLearningPathAsync(userPathId, firebaseUid);
                return Ok(newItems);
            }
            catch (UsageLimitExceededException ex)
            {
                return StatusCode(429, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                // Return 404 Not Found if the user or path doesn't exist
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        [Authorize]
        [HttpPost("{pathTemplateId}/rate")]
        public async Task<IActionResult> RatePath(int pathTemplateId, [FromBody] SubmitRatingDto dto)
        {
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (firebaseUid == null) return Unauthorized();

            var success = await _learningPathService.RatePathAsync(pathTemplateId, firebaseUid, dto.Rating);

            if (!success)
            {
                return NotFound(new { message = "Path template not found." });
            }

            return Ok(new { message = "Rating submitted successfully." });
        }

        [Authorize]
        [HttpPatch("items/{itemTemplateId}/toggle-completion")]
        public async Task<IActionResult> TogglePathItemCompletion(int itemTemplateId)
        {
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var updatedItem = await _learningPathService.TogglePathItemCompletionAsync(itemTemplateId, firebaseUid);
            if (updatedItem == null) return NotFound();
            return Ok(updatedItem);
        }

        [Authorize]
        [HttpPatch("resources/{resourceTemplateId}/toggle-completion")]
        public async Task<IActionResult> ToggleResourceCompletion(int resourceTemplateId)
        {
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var updatedResource = await _learningPathService.ToggleResourceCompletionAsync(resourceTemplateId, firebaseUid);
            if (updatedResource == null) return NotFound();
            return Ok(updatedResource);
        }

        [Authorize]
        [HttpDelete("{userPathId}")]
        public async Task<IActionResult> DeletePath(int userPathId)
        {
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var success = await _learningPathService.DeletePathAsync(userPathId, firebaseUid);
            if (!success)
            {
                return NotFound(new { message = $"Path with ID {userPathId} not found for this user." });
            }
            return NoContent();
        }
    }
}