using LearningAppNetCoreApi.DTOs;
using LearningAppNetCoreApi.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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

        [HttpGet]
        // [Authorize]
        public async Task<IActionResult> GetUserPaths()
        {
            var userAuth0Id = "temp-user-id-for-testing";
            // var userAuth0Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // if (string.IsNullOrEmpty(userAuth0Id))
            // {
            //     return Unauthorized();
            // }

            var paths = await _learningPathService.GetUserPathsAsync(userAuth0Id);
            return Ok(paths);
        }

        [HttpGet("{id}")]
        // [Authorize]
        public async Task<IActionResult> GetPathById(int id)
        {
            var path = await _learningPathService.GetPathByIdAsync(id);
            if (path == null)
            {
                return NotFound();
            }
            return Ok(path);
        }

        [HttpPost]
        // [Authorize] // This endpoint is protected and requires authentication
        public async Task<IActionResult> CreateLearningPath([FromBody] CreateLearningPathDto createDto)
        {
            // Temporarily hardcode a user ID for testing without authentication
            var userAuth0Id = "temp-user-id-for-testing";

            // The original code to get the user from the token:
            // var userAuth0Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // if (string.IsNullOrEmpty(userAuth0Id))
            // {
            //     return Unauthorized();
            // }

            try
            {
                var newPath = await _learningPathService.CreateLearningPathAsync(createDto.Prompt, userAuth0Id);
                return CreatedAtAction(nameof(GetPathById), new { id = newPath.Id }, newPath);
            }
            catch (InvalidOperationException ex)
            {
                // This catches the error for non-learning prompts
                return BadRequest(new { message = ex.Message });
            }
            catch (HttpRequestException ex)
            {
                // This catches errors from the API call itself
                return StatusCode(503, new { message = "Service unavailable. Could not connect to the AI service.", details = ex.Message });
            }
        }

        [HttpPost("{pathId}/extend")]
        // [Authorize]
        public async Task<IActionResult> ExtendLearningPath(int pathId)
        {
            try
            {
                var newItems = await _learningPathService.ExtendLearningPathAsync(pathId);
                if (newItems == null)
                {
                    return NotFound(new { message = $"Learning path with ID {pathId} not found." });
                }
                return Ok(newItems);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, new { message = "Service unavailable. Could not connect to the AI service.", details = ex.Message });
            }
            catch (JsonReaderException ex)
            {
                return StatusCode(502, new { message = "Bad gateway. The AI service returned an invalid response.", details = ex.Message });
            }
        }

        [HttpPatch("items/{itemId}/toggle-completion")]
        public async Task<IActionResult> TogglePathItemCompletion(int itemId)
        {
            var updatedItem = await _learningPathService.TogglePathItemCompletionAsync(itemId);
            if (updatedItem == null) return NotFound();
            return Ok(updatedItem);
        }

        [HttpPatch("resources/{resourceId}/toggle-completion")]
        public async Task<IActionResult> ToggleResourceCompletion(int resourceId)
        {
            var updatedResource = await _learningPathService.ToggleResourceCompletionAsync(resourceId);
            if (updatedResource == null) return NotFound();
            return Ok(updatedResource);
        }

        [HttpDelete("all")]
        // [Authorize]
        public async Task<IActionResult> DeleteAllUserPaths()
        {
            var userAuth0Id = "temp-user-id-for-testing";
            // var userAuth0Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // if (string.IsNullOrEmpty(userAuth0Id))
            // {
            //     return Unauthorized();
            // }

            await _learningPathService.DeleteAllUserPathsAsync(userAuth0Id);
            return NoContent(); // 204 No Content is a standard response for a successful deletion
        }
    }
}