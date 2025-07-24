using LearningAppNetCoreApi.Dtos;
using LearningAppNetCoreApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningAppNetCoreApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly IUserService _userService;

        public SubscriptionsController(ISubscriptionService subscriptionService, IUserService userService)
        {
            _subscriptionService = subscriptionService;
            _userService = userService;
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateSubscription([FromBody] UpdateSubscriptionDto dto)
        {
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (firebaseUid == null)
            {
                return Unauthorized();
            }

            var user = await _userService.GetUserByFirebaseUidAsync(firebaseUid);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var updatedUser = await _subscriptionService.UpdateUserSubscriptionAsync(user.Id, dto.Tier, dto.IsYearly);
            return Ok(updatedUser);
        }
    }
}
