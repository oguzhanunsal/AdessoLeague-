using AdessoLeague.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace AdessoLeague.Api.Extensions;

public static class ResultExtensions
{
    public static ActionResult<TValue> ToActionResult<TValue>(
        this Result<TValue> result,
        ControllerBase controller,
        Func<TValue, ActionResult<TValue>>? onSuccess = null)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(controller);

        if (result.IsSuccess)
        {
            return onSuccess is null ? controller.Ok(result.Value) : onSuccess(result.Value);
        }

        return controller.ToProblem(result.Error);
    }

    private static ActionResult ToProblem(this ControllerBase controller, Error error)
    {
        if (error is ValidationError validation)
        {
            return controller.ValidationProblem(new ValidationProblemDetails(ToDictionary(validation))
            {
                Title = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
                Instance = controller.HttpContext.Request.Path,
            });
        }

        var status = error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest,
        };

        return controller.Problem(
            detail: error.Message,
            instance: controller.HttpContext.Request.Path,
            statusCode: status,
            title: error.Code);
    }

    private static Dictionary<string, string[]> ToDictionary(ValidationError validation) => validation.Errors
        .GroupBy(error => error.Code, StringComparer.Ordinal)
        .ToDictionary(
            group => group.Key,
            group => group.Select(error => error.Message).ToArray(),
            StringComparer.Ordinal);
}
