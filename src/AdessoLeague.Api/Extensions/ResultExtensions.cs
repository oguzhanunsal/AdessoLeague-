using AdessoLeague.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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
            // Built through the factory rather than by hand so the "type" member and any registered
            // ProblemDetails customisation land on 400s exactly as they do on 404s and 409s.
            var modelState = new ModelStateDictionary();
            foreach (var failure in validation.Errors)
            {
                modelState.AddModelError(failure.Code, failure.Message);
            }

            return controller.ValidationProblem(controller.ProblemDetailsFactory.CreateValidationProblemDetails(
                controller.HttpContext,
                modelState,
                statusCode: StatusCodes.Status400BadRequest,
                instance: controller.HttpContext.Request.Path));
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

}
