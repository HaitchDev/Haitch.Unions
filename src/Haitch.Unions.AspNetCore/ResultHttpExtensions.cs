using Microsoft.AspNetCore.Http;

namespace Haitch.Unions.AspNetCore;

/// <summary>
/// Converts <see cref="Result{T}"/> values, and tasks producing them, to minimal API results.
/// </summary>
public static class ResultHttpExtensions
{
    extension<T>(Result<T> result)
        where T : notnull
    {
        /// <summary>
        /// Returns 200 OK with the value when Ok, otherwise the error's problem details.
        /// </summary>
        public IResult ToHttpResult() =>
            result.Match<IResult>(
                TypedResults.Ok,
                error => TypedResults.Problem(error.ToProblemDetails())
            );
    }

    extension(Result<Unit> result)
    {
        /// <summary>
        /// Returns 204 No Content when Ok, otherwise the error's problem details.
        /// </summary>
        public IResult ToHttpResult() =>
            result.Match<IResult>(
                _ => TypedResults.NoContent(),
                error => TypedResults.Problem(error.ToProblemDetails())
            );
    }

    extension<T>(Task<Result<T>> resultTask)
        where T : notnull
    {
        /// <summary>
        /// Awaits the result, then returns 200 OK with the value when Ok, otherwise the error's problem details.
        /// </summary>
        public async Task<IResult> ToHttpResultAsync() =>
            (await resultTask.ConfigureAwait(false)).ToHttpResult();
    }

    extension(Task<Result<Unit>> resultTask)
    {
        /// <summary>
        /// Awaits the result, then returns 204 No Content when Ok, otherwise the error's problem details.
        /// </summary>
        public async Task<IResult> ToHttpResultAsync() =>
            (await resultTask.ConfigureAwait(false)).ToHttpResult();
    }
}
