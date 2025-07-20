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
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

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
    }
}