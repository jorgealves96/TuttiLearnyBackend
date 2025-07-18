using LearningAppNetCoreApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningAppNetCoreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetUserStats()
        {
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(firebaseUid))
            {
                return Unauthorized();
            }

            var stats = await _profileService.GetUserStatsAsync(firebaseUid);
            if (stats == null)
            {
                // This can happen if the user exists in Firebase but not yet in our DB.
                // We can return default stats or an error.
                return NotFound(new { message = "User profile not found." });
            }

            return Ok(stats);
        }
    }
}