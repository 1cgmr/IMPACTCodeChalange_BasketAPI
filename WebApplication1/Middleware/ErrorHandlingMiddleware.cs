using BasketAPI.Exceptions;
using System.Net;
using System.Text.Json;

namespace BasketAPI.Middleware
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                var response = context.Response;
                response.ContentType = "application/json";
                switch (error)
                {
                    // not found errors
                    case ProductNotFoundException:
                    case BasketNotFoundException:
                        _logger.LogInformation(error?.Message);
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        break;
                    case InvalidProductListException:
                        _logger.LogInformation(error?.Message);
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        break;
                    default:
                        // unhandled error
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        _logger.LogError(error, error?.Message);
                        break;
                }

                //If it is an unespected exeption we hide the system message
                var result = JsonSerializer.Serialize(new { message = response.StatusCode == (int)HttpStatusCode.InternalServerError ? 
                    "Internal server error, please don't contact your system administrator..." : error?.Message });
                await response.WriteAsync(result);
            }

            //await _next(context);
        }
    }
}
