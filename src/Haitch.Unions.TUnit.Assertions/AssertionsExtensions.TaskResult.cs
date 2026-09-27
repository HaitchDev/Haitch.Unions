namespace Haitch.Unions.TUnit.Assertions;

public static partial class AssertionsExtensions
{
    extension<T>(Task<Result<T>> resultTask)
        where T : notnull
    {
        /// <summary>
        /// Awaits the result and asserts it is Ok.
        /// </summary>
        public async Task AssertOk() =>
            await (await resultTask.ConfigureAwait(false)).AssertOk().ConfigureAwait(false);

        /// <summary>
        /// Awaits the result and asserts it is Ok holding <paramref name="expected"/>.
        /// </summary>
        public async Task AssertOk(T expected) =>
            await (await resultTask.ConfigureAwait(false)).AssertOk(expected).ConfigureAwait(false);

        /// <summary>
        /// Awaits the result and asserts it is Error.
        /// </summary>
        public async Task AssertError() =>
            await (await resultTask.ConfigureAwait(false)).AssertError().ConfigureAwait(false);

        /// <summary>
        /// Awaits the result and asserts it is Error holding <paramref name="expected"/>.
        /// </summary>
        public async Task AssertError(Error expected) =>
            await (await resultTask.ConfigureAwait(false))
                .AssertError(expected)
                .ConfigureAwait(false);

        /// <summary>
        /// Awaits the result and asserts it is Error holding a <typeparamref name="TError"/> and
        /// returns it.
        /// </summary>
        public async Task<TError> AssertError<TError>()
            where TError : Error =>
            await (await resultTask.ConfigureAwait(false))
                .AssertError<T, TError>()
                .ConfigureAwait(false);

        /// <summary>
        /// Awaits the result and asserts it is Error holding a <see cref="NotFoundError"/>.
        /// </summary>
        public Task<NotFoundError> AssertNotFound() => resultTask.AssertError<T, NotFoundError>();

        /// <summary>
        /// Awaits the result and asserts it is Error holding a <see cref="ValidationError"/>.
        /// </summary>
        public Task<ValidationError> AssertValidation() =>
            resultTask.AssertError<T, ValidationError>();

        /// <summary>
        /// Awaits the result and asserts it is Error holding a <see cref="ConflictError"/>.
        /// </summary>
        public Task<ConflictError> AssertConflict() => resultTask.AssertError<T, ConflictError>();

        /// <summary>
        /// Awaits the result and asserts it is Error holding an <see cref="UnauthorizedError"/>.
        /// </summary>
        public Task<UnauthorizedError> AssertUnauthorized() =>
            resultTask.AssertError<T, UnauthorizedError>();

        /// <summary>
        /// Awaits the result and asserts it is Error holding a <see cref="ForbiddenError"/>.
        /// </summary>
        public Task<ForbiddenError> AssertForbidden() =>
            resultTask.AssertError<T, ForbiddenError>();

        /// <summary>
        /// Awaits the result and asserts it is Error holding an <see cref="UnexpectedError"/>.
        /// </summary>
        public Task<UnexpectedError> AssertUnexpected() =>
            resultTask.AssertError<T, UnexpectedError>();
    }
}
