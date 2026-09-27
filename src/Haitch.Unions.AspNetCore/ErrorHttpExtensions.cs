using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Haitch.Unions.AspNetCore;

/// <summary>
/// Maps <see cref="Error"/> values to HTTP status codes and problem details.
/// </summary>
public static class ErrorHttpExtensions
{
    extension(Error error)
    {
        /// <summary>
        /// Gets the HTTP status code for this error, preferring <see cref="IHttpError.StatusCode"/> over its kind.
        /// </summary>
        public int ToStatusCode() =>
            error switch
            {
                IHttpError httpError => httpError.StatusCode,
                NotFoundError => StatusCodes.Status404NotFound,
                ValidationError => StatusCodes.Status400BadRequest,
                ConflictError => StatusCodes.Status409Conflict,
                UnauthorizedError => StatusCodes.Status401Unauthorized,
                ForbiddenError => StatusCodes.Status403Forbidden,
                UnexpectedError => StatusCodes.Status500InternalServerError,
                // A plain Error is an anticipated problem; UnexpectedError covers the 500 case.
                _ => StatusCodes.Status400BadRequest,
            };

        /// <summary>
        /// Creates problem details describing this error.
        /// </summary>
        public ProblemDetails ToProblemDetails()
        {
            var problem = error is ValidationError validation
                ? new HttpValidationProblemDetails(validation.Failures.ToDictionary())
                : new ProblemDetails();
            problem.Status = error.ToStatusCode();
            problem.Detail = error.Description;
            problem.Extensions["code"] = error.Code;
            return problem;
        }
    }
}
