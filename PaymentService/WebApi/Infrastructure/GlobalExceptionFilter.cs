using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using FluentValidation;

namespace PaymentService.WebApi.Infrastructure;
public class GlobalExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is ValidationException valEx)
        {
            var errors = valEx.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray());

            context.Result = new BadRequestObjectResult(new { Errors = errors });
            context.ExceptionHandled = true;
        }
    }
}
