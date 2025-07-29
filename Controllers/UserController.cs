using LearningAppNetCoreApi.Dtos;
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

        [HttpGet("me/subscription-status")]
        public async Task<IActionResult> GetMySubscriptionStatus()
        {
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (firebaseUid == null) return Unauthorized();

            var status = await _userService.GetUserSubscriptionStatusAsync(firebaseUid);
            if (status == null) return NotFound();

            return Ok(status);
        }
    }
}