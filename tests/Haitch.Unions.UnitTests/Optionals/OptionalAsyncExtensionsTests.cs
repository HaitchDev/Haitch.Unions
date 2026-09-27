using Haitch.Unions.TUnit.Assertions;

namespace Haitch.Unions.UnitTests.Optionals;

public class OptionalAsyncExtensionsTests
{
    private static readonly Error TestError = new("Test.Error", "Something went wrong");

    private static async Task<Optional<int>> SomeAsync(int value)
    {
        await Task.Yield();
        return value;
    }

    private static async Task<Optional<int>> NoneAsync()
    {
        await Task.Yield();
        return Optional<int>.None();
    }

    private static async Task<Optional<string>> SomeStringAsync(string value)
    {
        await Task.Yield();
        return value;
    }

    private static async Task<Optional<string>> NoneStringAsync()
    {
        await Task.Yield();
        return Optional<string>.None();
    }

    private static async Task<T> YieldAsync<T>(T value)
    {
        await Task.Yield();
        return value;
    }

    [Test]
    public async Task ShouldMapAsyncWhenSome()
    {
        Optional<int> optional = 2;
        await optional.MapAsync(value => YieldAsync(value * 10)).AssertSome(20);
    }

    [Test]
    public async Task ShouldStayNoneThroughMapAsyncWithoutCallingIt()
    {
        var called = false;

        var mapped = Optional<int>
            .None()
            .MapAsync(value =>
            {
                called = true;
                return YieldAsync(value);
            });

        await mapped.AssertNone();
        await Assert.That(called).IsFalse();
    }

    [Test]
    public async Task ShouldBindAsyncWhenSome()
    {
        Optional<int> optional = 2;
        await optional.BindAsync(value => SomeAsync(value * 10)).AssertSome(20);
    }

    [Test]
    public async Task ShouldStayNoneThroughBindAsync()
    {
        await Optional<int>.None().BindAsync(SomeAsync).AssertNone();
    }

    [Test]
    public async Task ShouldKeepValueWhenEnsureAsyncPasses()
    {
        Optional<int> optional = 5;
        await optional.EnsureAsync(value => YieldAsync(value > 0)).AssertSome(5);
    }

    [Test]
    public async Task ShouldBecomeNoneWhenEnsureAsyncFails()
    {
        Optional<int> optional = -5;
        await optional.EnsureAsync(value => YieldAsync(value > 0)).AssertNone();
    }

    [Test]
    public async Task ShouldTapAsyncWhenSome()
    {
        Optional<int> optional = 1;
        var tapped = 0;

        await optional.TapAsync(async value =>
        {
            await Task.Yield();
            tapped = value;
        });

        await Assert.That(tapped).IsEqualTo(1);
    }

    [Test]
    public async Task ShouldNotTapAsyncWhenNone()
    {
        var called = false;

        await Optional<int>
            .None()
            .TapAsync(async _ =>
            {
                await Task.Yield();
                called = true;
            });

        await Assert.That(called).IsFalse();
    }

    [Test]
    public async Task ShouldTapNoneAsyncWhenNone()
    {
        var called = false;

        await Optional<int>
            .None()
            .TapNoneAsync(async () =>
            {
                await Task.Yield();
                called = true;
            });

        await Assert.That(called).IsTrue();
    }

    [Test]
    public async Task ShouldNotTapNoneAsyncWhenSome()
    {
        var called = false;
        Optional<int> optional = 1;

        await optional.TapNoneAsync(async () =>
        {
            await Task.Yield();
            called = true;
        });

        await Assert.That(called).IsFalse();
    }

    [Test]
    public async Task ShouldKeepValueThroughOrElseAsyncWithoutCallingAlternative()
    {
        Optional<int> optional = 1;
        var called = false;

        var result = optional.OrElseAsync(() =>
        {
            called = true;
            return SomeAsync(2);
        });

        await result.AssertSome(1);
        await Assert.That(called).IsFalse();
    }

    [Test]
    public async Task ShouldUseAlternativeFromOrElseAsyncWhenNone()
    {
        await Optional<int>.None().OrElseAsync(() => SomeAsync(2)).AssertSome(2);
    }

    [Test]
    public async Task ShouldMatchAsyncWhenSome()
    {
        Optional<int> optional = 2;
        var matched = await optional.MatchAsync(value => YieldAsync(value * 10), () => YieldAsync(-1));
        await Assert.That(matched).IsEqualTo(20);
    }

    [Test]
    public async Task ShouldMatchAsyncWhenNone()
    {
        var matched = await Optional<int>
            .None()
            .MatchAsync(value => YieldAsync(value * 10), () => YieldAsync(-1));
        await Assert.That(matched).IsEqualTo(-1);
    }

    [Test]
    public async Task ShouldMapTaskOptional()
    {
        await SomeAsync(2).Map(value => value * 10).AssertSome(20);
    }

    [Test]
    public async Task ShouldMapAsyncTaskOptional()
    {
        await SomeAsync(2).MapAsync(value => YieldAsync(value * 10)).AssertSome(20);
    }

    [Test]
    public async Task ShouldBindTaskOptional()
    {
        await SomeAsync(2).Bind(value => Optional.Some(value * 10)).AssertSome(20);
    }

    [Test]
    public async Task ShouldBindAsyncTaskOptional()
    {
        await SomeAsync(2).BindAsync(value => SomeAsync(value * 10)).AssertSome(20);
    }

    [Test]
    public async Task ShouldEnsureTaskOptional()
    {
        await SomeAsync(-5).Ensure(value => value > 0).AssertNone();
    }

    [Test]
    public async Task ShouldEnsureAsyncTaskOptional()
    {
        await SomeAsync(-5).EnsureAsync(value => YieldAsync(value > 0)).AssertNone();
    }

    [Test]
    public async Task ShouldAwaitAsyncLambdaPassedToTapOnTaskOptional()
    {
        Optional<int> optional = 1;
        var ran = false;

        await Task.FromResult(optional)
            .TapAsync(async _ =>
            {
                await Task.Yield();
                ran = true;
            });

        await Assert.That(ran).IsTrue();
    }

    [Test]
    public async Task ShouldTapAsyncTaskOptional()
    {
        var tapped = 0;

        await SomeAsync(1)
            .TapAsync(async value =>
            {
                await Task.Yield();
                tapped = value;
            });

        await Assert.That(tapped).IsEqualTo(1);
    }

    [Test]
    public async Task ShouldTapNoneAsyncTaskOptional()
    {
        var called = false;

        await NoneAsync()
            .TapNoneAsync(async () =>
            {
                await Task.Yield();
                called = true;
            });

        await Assert.That(called).IsTrue();
    }

    [Test]
    public async Task ShouldNotTapNoneAsyncTaskOptionalWhenSome()
    {
        var called = false;

        await SomeAsync(1)
            .TapNoneAsync(async () =>
            {
                await Task.Yield();
                called = true;
            });

        await Assert.That(called).IsFalse();
    }

    [Test]
    public async Task ShouldGetValueOrDefaultFromTaskOptional()
    {
        await Assert.That(await NoneAsync().GetValueOrDefault(-1)).IsEqualTo(-1);
    }

    [Test]
    public async Task ShouldOrElseTaskOptional()
    {
        await NoneAsync().OrElse(() => Optional.Some(2)).AssertSome(2);
    }

    [Test]
    public async Task ShouldOrElseAsyncTaskOptional()
    {
        await NoneAsync().OrElseAsync(() => SomeAsync(2)).AssertSome(2);
    }

    [Test]
    public async Task ShouldMatchTaskOptionalWhenSome()
    {
        var matched = await SomeAsync(2).Match(value => value * 10, () => -1);
        await Assert.That(matched).IsEqualTo(20);
    }

    [Test]
    public async Task ShouldMatchTaskOptionalWhenNone()
    {
        var matched = await NoneAsync().Match(value => value * 10, () => -1);
        await Assert.That(matched).IsEqualTo(-1);
    }

    [Test]
    public async Task ShouldMatchAsyncTaskOptionalWhenSome()
    {
        var matched = await SomeAsync(2)
            .MatchAsync(value => YieldAsync(value * 10), () => YieldAsync(-1));
        await Assert.That(matched).IsEqualTo(20);
    }

    [Test]
    public async Task ShouldMatchAsyncTaskOptionalWhenNone()
    {
        var matched = await NoneAsync()
            .MatchAsync(value => YieldAsync(value * 10), () => YieldAsync(-1));
        await Assert.That(matched).IsEqualTo(-1);
    }

    [Test]
    public async Task ShouldConvertTaskOptionalToResult()
    {
        await NoneAsync().ToResult(TestError).AssertError(TestError);
    }

    [Test]
    public void ShouldThrowWhenToResultGivenNullErrorOnTaskOptional()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            _ = SomeAsync(1).ToResult(null!);
        });
    }

    [Test]
    public async Task ShouldConvertSomeTaskOptionalToNullable()
    {
        await Assert.That(await SomeAsync(1).ToNullable()).IsEqualTo(1);
    }

    [Test]
    public async Task ShouldConvertNoneTaskOptionalToNull()
    {
        await Assert.That(await NoneAsync().ToNullable()).IsNull();
    }

    [Test]
    public async Task ShouldConvertSomeReferenceTypeTaskOptionalToNullable()
    {
        await Assert.That(await SomeStringAsync("value").ToNullable()).IsEqualTo("value");
    }

    [Test]
    public async Task ShouldConvertNoneReferenceTypeTaskOptionalToNull()
    {
        await Assert.That(await NoneStringAsync().ToNullable()).IsNull();
    }

    [Test]
    public async Task ShouldChainTaskOptionalIntoResult()
    {
        var result = SomeAsync(2)
            .MapAsync(value => YieldAsync(value * 10))
            .Ensure(value => value > 0)
            .ToResult(TestError)
            .Map(value => value + 1);

        await result.AssertOk(21);
    }
}
