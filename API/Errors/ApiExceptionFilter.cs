using Domain.Resources;
using Application.Files;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace API.Errors;

public sealed class ApiExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var exception = context.Exception;
        var status = exception switch
        {
            FileTooLargeException => StatusCodes.Status413PayloadTooLarge,
            UnsupportedFileException => StatusCodes.Status415UnsupportedMediaType,
            ArgumentException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            InvalidOperationException => StatusCodes.Status409Conflict,
            DbUpdateConcurrencyException => StatusCodes.Status409Conflict,
            DbUpdateException { InnerException: SqlException { Number: 2601 or 2627 } } => StatusCodes.Status409Conflict,
            _ => 0
        };

        if (status == 0)
            return;

        var language = ApiLanguage.Resolve(context.HttpContext);
        var title = ErrorMessages.Get(status switch
        {
            StatusCodes.Status400BadRequest => ErrorCode.BadRequestTitle,
            StatusCodes.Status404NotFound => ErrorCode.NotFoundTitle,
            StatusCodes.Status413PayloadTooLarge => ErrorCode.PayloadTooLargeTitle,
            StatusCodes.Status415UnsupportedMediaType => ErrorCode.UnsupportedMediaTypeTitle,
            _ => ErrorCode.ConflictTitle
        }, language);

        var detail = exception is DbUpdateException
            ? ErrorMessages.Get(ErrorCode.ResourceConflict, language)
            : exception.Message.Split(Environment.NewLine)[0];
        if (exception is ArgumentException { ParamName: not null })
        {
            var parameterSuffix = detail.LastIndexOf(" (", StringComparison.Ordinal);
            if (parameterSuffix >= 0 && detail.EndsWith(')'))
                detail = detail[..parameterSuffix];
        }

        context.Result = new ObjectResult(new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail
        }) { StatusCode = status };
        context.ExceptionHandled = true;
    }
}
