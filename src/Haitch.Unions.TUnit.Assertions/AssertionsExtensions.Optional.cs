namespace Haitch.Unions.TUnit.Assertions;

public static partial class AssertionsExtensions
{
    extension<T>(Optional<T> optional)
        where T : notnull
    {
        /// <summary>
        /// Asserts the optional holds a value.
        /// </summary>
        public async Task AssertSome()
        {
            await Assert.That(optional.IsSome).IsTrue().Because($"optional was {optional}");
        }

        /// <summary>
        /// Asserts the optional holds <paramref name="expected"/>.
        /// </summary>
        public async Task AssertSome(T expected)
        {
            await optional.AssertSome().ConfigureAwait(false);
            optional.TryGetValue(out T? value);
            await Assert.That(value).IsEqualTo(expected);
        }

        /// <summary>
        /// Asserts the optional is empty.
        /// </summary>
        public async Task AssertNone()
        {
            await Assert.That(optional.IsSome).IsFalse().Because($"optional was {optional}");
        }
    }
}
