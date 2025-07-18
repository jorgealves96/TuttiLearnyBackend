using LearningAppNetCoreApi.DTOs;
using LearningAppNetCoreApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPathById(int id)
        {
            // Note: For full security, you should also verify that the user owns this path.
            // This can be done in the service layer.
            var path = await _learningPathService.GetPathByIdAsync(id);
            if (path == null)
            {
                return NotFound();
            }
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

#if DEBUG
            // If running in Debug mode and no token is provided, use a test user ID.
            if (string.IsNullOrEmpty(firebaseUid))
            {
                firebaseUid = "temp-firebase-uid-for-testing";
            }
#endif

            var newPath = await _learningPathService.GenerateNewPathAsync(dto.Prompt, firebaseUid);
            return CreatedAtAction(nameof(GetPathById), new { id = newPath.Id }, newPath);
        }

        [Authorize]
        // New endpoint for assigning a copy of an existing path
        [HttpPost("{id}/assign")]
        public async Task<IActionResult> AssignPath(int id)
        {
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

#if DEBUG
            // If running in Debug mode and no token is provided, use a test user ID.
            if (string.IsNullOrEmpty(firebaseUid))
            {
                firebaseUid = "temp-firebase-uid-for-testing";
            }
#endif

            var assignedPath = await _learningPathService.AssignPathToUserAsync(id, firebaseUid);
            if (assignedPath == null) return NotFound();
            return CreatedAtAction(nameof(GetPathById), new { id = assignedPath.Id }, assignedPath);
        }

        [Authorize]
        [HttpPost("{pathId}/extend")]
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

        [Authorize]
        [HttpPatch("items/{itemId}/toggle-completion")]
        public async Task<IActionResult> TogglePathItemCompletion(int itemId)
        {
            var updatedItem = await _learningPathService.TogglePathItemCompletionAsync(itemId);
            if (updatedItem == null) return NotFound();
            return Ok(updatedItem);
        }

        [Authorize]
        [HttpPatch("resources/{resourceId}/toggle-completion")]
        public async Task<IActionResult> ToggleResourceCompletion(int resourceId)
        {
            var updatedResource = await _learningPathService.ToggleResourceCompletionAsync(resourceId);
            if (updatedResource == null) return NotFound();
            return Ok(updatedResource);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePath(int id)
        {
            var success = await _learningPathService.DeletePathAsync(id);
            if (!success)
            {
                return NotFound(new { message = $"Path with ID {id} not found." });
            }
            return NoContent();
        }

        [Authorize]
        [HttpDelete("all")]
        public async Task<IActionResult> DeleteAllUserPaths()
        {
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(firebaseUid))
            {
                return Unauthorized();
            }

            await _learningPathService.DeleteAllUserPathsAsync(firebaseUid);
            return NoContent();
        }
    }
}