namespace Haitch.Unions.TUnit.Assertions;

public static partial class AssertionsExtensions
{
    extension<T>(Task<Optional<T>> optionalTask)
        where T : notnull
    {
        /// <summary>
        /// Awaits the optional and asserts it holds a value.
        /// </summary>
        public async Task AssertSome() =>
            await (await optionalTask.ConfigureAwait(false)).AssertSome().ConfigureAwait(false);

        /// <summary>
        /// Awaits the optional and asserts it holds <paramref name="expected"/>.
        /// </summary>
        public async Task AssertSome(T expected) =>
            await (await optionalTask.ConfigureAwait(false))
                .AssertSome(expected)
                .ConfigureAwait(false);

        /// <summary>
        /// Awaits the optional and asserts it is empty.
        /// </summary>
        public async Task AssertNone() =>
            await (await optionalTask.ConfigureAwait(false)).AssertNone().ConfigureAwait(false);
    }
}
