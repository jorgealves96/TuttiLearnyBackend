using LearningAppNetCoreApi.Dtos;
using LearningAppNetCoreApi.Models;
using LearningAppNetCoreApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningAppNetCoreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize]
        [HttpPost("sync")]
        public async Task<IActionResult> SyncUser()
        {
            // The User object here is the ClaimsPrincipal from the validated token
            var user = await _userService.SyncUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }
            // We can just return Ok, the main purpose is to ensure the user exists in the DB.
            return Ok(new { message = "User synchronized successfully." });
        }

        [Authorize]
        [HttpPatch("name")]
        public async Task<IActionResult> UpdateName([FromBody] UpdateUserDto dto)
        {
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(firebaseUid))
            {
                return Unauthorized();
            }

            var updatedUser = await _userService.UpdateUserNameAsync(firebaseUid, dto.NewName);
            if (updatedUser == null)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(updatedUser);
        }

        [Authorize]
        [HttpDelete("me")]
        public async Task<IActionResult> DeleteCurrentUser()
        {
            // Get the user's Firebase UID from the token
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(firebaseUid))
            {
                return Unauthorized("User UID not found in token.");
            }

            var success = await _userService.DeleteUserAsync(firebaseUid);

            if (success)
            {
                return Ok(new { message = "User account deleted successfully." });
            }

            return StatusCode(500, "An error occurred while deleting the user account.");
        }

        //[Authorize]
        [HttpDelete("{firebaseUid}")]
        //[Authorize(Policy = "AdminOnly")] // This ensures only admins can use it, implement in prod.
        public async Task<IActionResult> DeleteUserByUid(string firebaseUid)
        {
            if (string.IsNullOrEmpty(firebaseUid))
            {
                return BadRequest("Firebase UID cannot be empty.");
            }

            var success = await _userService.DeleteUserAsync(firebaseUid);

            if (success)
            {
                return Ok(new { message = $"User with UID '{firebaseUid}' deleted successfully." });
            }

            // The user might not have existed in the first place, but the goal is achieved.
            // For an admin endpoint, returning success is often acceptable.
            // You could also return NotFound() if success is false.
            return Ok(new { message = $"User with UID '{firebaseUid}' was not found or already deleted." });
        }
    }
}