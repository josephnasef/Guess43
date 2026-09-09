using FluentValidation;
using Guess43.Api.Errors;
using Guess43.Application.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Guess43.Api.Validation;

/// <summary>
/// Validates action arguments that have a registered FluentValidation validator and
/// returns a VALIDATION_ERROR ProblemDetails on failure.
/// </summary>
public sealed class ValidationFilter(IServiceProvider serviceProvider) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (serviceProvider.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var validationContext = new ValidationContext<object>(argument);
            var result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);
            if (result.IsValid)
            {
                continue;
            }

            var error = ApplicationErrors.Validation("One or more validation errors occurred.");
            var problem = ErrorResults.ToProblemDetails(error, context.HttpContext);
            problem.Extensions["errors"] = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            context.Result = new ObjectResult(problem) { StatusCode = problem.Status };
            return;
        }

        await next();
    }
}
