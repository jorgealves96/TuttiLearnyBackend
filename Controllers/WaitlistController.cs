using LearningAppNetCoreApi.Dtos;
using LearningAppNetCoreApi.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

// TODO: Remove after app is not on waitlist anymore

namespace LearningAppNetCoreApi.Controllers
{
    [ApiController]
    [Route("api/waitlist")]
    [EnableCors("AllowWebApp")]
    public class WaitlistController : ControllerBase
    {
        private readonly IWaitlistService _waitlistService;
        private readonly ILogger<WaitlistController> _logger;

        public WaitlistController(IWaitlistService waitlistService, ILogger<WaitlistController> logger)
        {
            _waitlistService = waitlistService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> JoinWaitlist([FromBody] WaitlistRequestDto request)
        {
            // The [ApiController] attribute automatically handles model state validation
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _waitlistService.AddEmailAsync(request);
                _logger.LogInformation("New email added to waitlist: {Email}", request.Email);
                return Ok(new { message = "Successfully joined the waitlist." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding email to waitlist: {Email}", request.Email);
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }
    }
}
