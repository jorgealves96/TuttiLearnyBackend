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
    [Authorize]
    public class PathsController : ControllerBase
    {
        private readonly ILearningPathService _learningPathService;

        public PathsController(ILearningPathService learningPathService)
        {
            _learningPathService = learningPathService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserPaths()
        {
            // Get the user's unique ID from the token's 'sub' (subject) claim
            var userAuth0Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userAuth0Id))
            {
                return Unauthorized();
            }

            var paths = await _learningPathService.GetUserPathsAsync(userAuth0Id);
            return Ok(paths);
        }

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

        [HttpPost]
        public async Task<IActionResult> CreateLearningPath([FromBody] CreateLearningPathDto createDto)
        {
            var userAuth0Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // Get the user's name and email from the token
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(userAuth0Id))
            {
                return Unauthorized();
            }

            try
            {
                // Pass the user details to the service
                var newPathDto = await _learningPathService.CreateLearningPathAsync(createDto.Prompt, userAuth0Id, userName, userEmail);
                return CreatedAtAction(nameof(GetPathById), new { id = newPathDto.Id }, newPathDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", details = ex.Message });
            }
        }

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

        [HttpDelete("all")]
        public async Task<IActionResult> DeleteAllUserPaths()
        {
            var userAuth0Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userAuth0Id))
            {
                return Unauthorized();
            }

            await _learningPathService.DeleteAllUserPathsAsync(userAuth0Id);
            return NoContent();
        }
    }
}