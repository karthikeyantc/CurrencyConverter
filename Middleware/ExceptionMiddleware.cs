namespace CurrencyConverter.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (ArgumentException ex)
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                _logger.LogWarning("Bad request: {Message}", ex.Message);

                await httpContext.Response.WriteAsJsonAsync(new Model.ErrorResponse
                {
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                _logger.LogError(ex, "An unexpected error occurred.");

                await httpContext.Response.WriteAsJsonAsync(new Model.ErrorResponse
                {
                    Message = "An unexpected error occurred. Please try again later."
                });
            }
        }
    }
}