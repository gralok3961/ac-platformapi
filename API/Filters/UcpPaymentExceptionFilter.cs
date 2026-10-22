using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Filters
{
    public sealed class UcpPaymentModelStateFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // JSON conversion errors may include raw input values. This filter is scoped only to
            // ProcessPayment and runs before ApiController's automatic model-state response.
            if (!context.ModelState.IsValid)
                context.Result = new BadRequestObjectResult(new
                {
                    code = "invalid_payment_request",
                    content = "Payment request JSON does not match the required fields or types."
                });
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }

    public sealed class UcpPaymentExceptionFilter(ILogger<UcpPaymentExceptionFilter> logger) : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is UcpPaymentRequestException paymentError)
            {
                context.Result = new ObjectResult(new { code = paymentError.Code, content = paymentError.Message })
                {
                    StatusCode = paymentError.StatusCode
                };
                context.ExceptionHandled = true;
                return;
            }

            var forbidden = context.Exception is UnauthorizedAccessException;

            if (!forbidden) logger.LogError(context.Exception, "UCP payment request failed.");

            context.Result = new ObjectResult(new
            {
                code = forbidden ? "authorization_denied" : "internal_error",
                content = forbidden ? "Authorization denied." : "Internal server error."
            })
            {
                StatusCode = forbidden ? 403 : 500
            };

            context.ExceptionHandled = true;
        }
    }
}
