using LearningAppNetCoreApi.Dtos;
using LearningAppNetCoreApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LearningAppNetCoreApi.Controllers
{
    [ApiController]
    [Route("api/webhooks")]
    public class WebhooksController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly ILogger<WebhooksController> _logger;

        public WebhooksController(ISubscriptionService subscriptionService, ILogger<WebhooksController> logger)
        {
            _subscriptionService = subscriptionService;
            _logger = logger;
        }

        [HttpPost("revenuecat")]
        public async Task<IActionResult> HandleRevenueCatWebhook([FromBody] RevenueCatWebhookDto webhook)
        {
            if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                return Unauthorized("Authorization header is missing.");
            }

            var expectedSecret = Environment.GetEnvironmentVariable("RevenueCatWebhookSecret");

            if (authHeader != $"Bearer {expectedSecret}")
            {
                return Unauthorized("Invalid Authorization header.");
            }

            var evt = webhook.Event;
            if (evt == null) return BadRequest();

            switch (evt.Type)
            {
                case "INITIAL_PURCHASE":
                case "RENEWAL":
                case "UNCANCELLATION":
                case "PRODUCT_CHANGE":
                    // For any event that grants or extends access, update the user's status.
                    await _subscriptionService.UpdateSubscriptionFromWebhookAsync(
                        evt.AppUserId,
                        evt.ProductId,
                        evt.ExpirationAtMs
                    );
                    _logger.LogInformation("Processed {EventType} for user {AppUserId}", evt.Type, evt.AppUserId);
                    break;

                case "EXPIRATION":
                case "CANCELLATION":
                    // For events that lead to losing access, you can log them for analytics
                    // or trigger a downgrade. Your daily cron job already handles the downgrade,
                    // so just logging it is a great start.
                    _logger.LogInformation("Received {EventType} for user {AppUserId}", evt.Type, evt.AppUserId);
                    break;

                default:
                    _logger.LogInformation("Received unhandled RevenueCat event type: {EventType}", evt.Type);
                    break;
            }

            return Ok();
        }
    }
}
