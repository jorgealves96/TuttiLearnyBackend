using System.Security.Claims;

namespace LearningAppNetCoreApi.Middleware
{
    public class StructuredLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<StructuredLoggingMiddleware> _logger;

        public StructuredLoggingMiddleware(RequestDelegate next, ILogger<StructuredLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Find the Firebase UID claim from the authenticated user
            var firebaseUid = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Create a dictionary of properties to add to the log scope
            var logContext = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(firebaseUid))
            {
                logContext["firebase_uid"] = firebaseUid;
            }

            // Begin a new log scope with our custom properties
            using (_logger.BeginScope(logContext))
            {
                // Pass the request to the next middleware in the pipeline
                await _next(context);
            }
        }
    }
}
