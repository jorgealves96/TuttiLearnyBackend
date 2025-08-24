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
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<PathsController> _logger;

        public UsersController(IUserService userService, ILogger<PathsController> logger)
        {
            _userService = userService;
            _logger = logger;
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

        [Authorize]
        [HttpGet("me/settings")]
        public async Task<IActionResult> GetUserSettings()
        {
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(firebaseUid))
            {
                return Unauthorized("User UID not found in token.");
            }

            var settings = await _userService.GetUserSettingsAsync(firebaseUid);
            if (settings == null)
            {
                return NotFound("User not found.");
            }

            return Ok(settings);
        }

        [Authorize]
        [HttpPost("sync")]
        public async Task<IActionResult> SyncUser()
        {
            try
            {
                var user = await _userService.SyncUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                return Ok(new { message = "User synchronized successfully." });
            }
            catch (AccountInCooldownException ex)
            {
                // Return a 409 Conflict status with a clear error message
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during user sync.");
                return StatusCode(500, new { message = "An internal server error occurred." });
            }
        }

        [Authorize]
        [HttpPost("me/fcm-token")]
        public async Task<IActionResult> UpdateFcmToken([FromBody] UpdateFcmTokenDto dto)
        {
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (firebaseUid == null) return Unauthorized();

            await _userService.UpdateFcmTokenAsync(firebaseUid, dto.FcmToken);
            return Ok();
        }

        [Authorize]
        [HttpPost("me/notification-preference")]
        public async Task<IActionResult> UpdateNotificationPreference([FromBody] UpdateNotificationPreferenceDto dto)
        {
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (firebaseUid == null) return Unauthorized();

            var success = await _userService.UpdateNotificationPreferenceAsync(firebaseUid, dto.IsEnabled);

            if (!success)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(new { message = "Notification preference updated successfully." });
        }

        [Authorize]
        [HttpPatch("me/path-generation-settings")]
        public async Task<IActionResult> UpdatePathGenerationSettings([FromBody] UpdatePathGenerationSettingsDto request)
        {
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(firebaseUid))
            {
                return Unauthorized("User UID not found in token.");
            }

            var success = await _userService.UpdatePathGenerationSettingsAsync(firebaseUid, request);

            if (success)
            {
                return Ok(new { message = "Path generation settings updated successfully." });
            }

            return NotFound(new { message = "User not found." });
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
        [HttpPost("restore")]
        public async Task<IActionResult> RestoreUser()
        {
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(firebaseUid)) return Unauthorized();

            var success = await _userService.RestoreUserAsync(firebaseUid);
            if (success)
            {
                return Ok(new { message = "Account restored successfully." });
            }
            return BadRequest(new { message = "Failed to restore account." });
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

            var success = await _userService.SoftDeleteAccountAsync(firebaseUid);

            if (success)
            {
                return Ok(new { message = "User account deleted successfully." });
            }

            return StatusCode(500, "An error occurred while deleting the user account.");
        }
    }
}