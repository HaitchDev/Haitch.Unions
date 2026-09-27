namespace Haitch.Unions.TUnit.Assertions;

/// <summary>
/// TUnit assertions for <see cref="Result{T}"/> and <see cref="Optional{T}"/>.
/// </summary>
public static partial class AssertionsExtensions
{
    extension<T>(Result<T> result)
        where T : notnull
    {
        /// <summary>
        /// Asserts the result is Ok.
        /// </summary>
        public async Task AssertOk()
        {
            await Assert.That(result.IsOk).IsTrue().Because($"result was {result}");
        }

        /// <summary>
        /// Asserts the result is Ok holding <paramref name="expected"/>.
        /// </summary>
        public async Task AssertOk(T expected)
        {
            await result.AssertOk().ConfigureAwait(false);
            result.TryGetValue(out T? value);
            await Assert.That(value).IsEqualTo(expected);
        }

        /// <summary>
        /// Asserts the result is Error.
        /// </summary>
        public async Task AssertError()
        {
            await Assert.That(result.IsOk).IsFalse().Because($"result was {result}");
        }

        /// <summary>
        /// Asserts the result is Error holding <paramref name="expected"/>.
        /// </summary>
        public async Task AssertError(Error expected)
        {
            await result.AssertError().ConfigureAwait(false);
            result.TryGetValue(out Error? error);
            await Assert.That(error).IsEqualTo(expected);
        }

        /// <summary>
        /// Asserts the result is Error holding a <typeparamref name="TError"/> and returns it.
        /// </summary>
        public async Task<TError> AssertError<TError>()
            where TError : Error
        {
            await result.AssertError().ConfigureAwait(false);
            result.TryGetValue(out Error? error);
            await Assert.That(error).IsTypeOf<TError>();
            return (TError)error!;
        }

        /// <summary>
        /// Asserts the result is Error holding a <see cref="NotFoundError"/>.
        /// </summary>
        public Task<NotFoundError> AssertNotFound() => result.AssertError<T, NotFoundError>();

        /// <summary>
        /// Asserts the result is Error holding a <see cref="ValidationError"/>.
        /// </summary>
        public Task<ValidationError> AssertValidation() =>
            result.AssertError<T, ValidationError>();

        /// <summary>
        /// Asserts the result is Error holding a <see cref="ConflictError"/>.
        /// </summary>
        public Task<ConflictError> AssertConflict() => result.AssertError<T, ConflictError>();

        /// <summary>
        /// Asserts the result is Error holding an <see cref="UnauthorizedError"/>.
        /// </summary>
        public Task<UnauthorizedError> AssertUnauthorized() =>
            result.AssertError<T, UnauthorizedError>();

        /// <summary>
        /// Asserts the result is Error holding a <see cref="ForbiddenError"/>.
        /// </summary>
        public Task<ForbiddenError> AssertForbidden() => result.AssertError<T, ForbiddenError>();

        /// <summary>
        /// Asserts the result is Error holding an <see cref="UnexpectedError"/>.
        /// </summary>
        public Task<UnexpectedError> AssertUnexpected() =>
            result.AssertError<T, UnexpectedError>();
    }
}
