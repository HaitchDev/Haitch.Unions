using Haitch.Unions.TUnit.Assertions;

namespace Haitch.Unions.UnitTests.Results;

public class ResultAsyncExtensionsTests
{
    private static readonly Error TestError = new("Test.Error", "Something went wrong");
    private static readonly Error OtherError = new("Other.Error", "Something else went wrong");

    private static async Task<Result<int>> OkAsync(int value)
    {
        await Task.Yield();
        return value;
    }

    private static async Task<Result<int>> FailAsync(Error error)
    {
        await Task.Yield();
        return error;
    }

    private static async Task<T> YieldAsync<T>(T value)
    {
        await Task.Yield();
        return value;
    }

    [Test]
    public async Task ShouldMapAsyncOnOk()
    {
        Result<int> result = 2;
        await result.MapAsync(value => YieldAsync(value * 10)).AssertOk(20);
    }

    [Test]
    public async Task ShouldPassErrorThroughMapAsyncWithoutCallingIt()
    {
        Result<int> result = TestError;
        var called = false;

        var mapped = result.MapAsync(value =>
        {
            called = true;
            return YieldAsync(value);
        });

        await mapped.AssertError(TestError);
        await Assert.That(called).IsFalse();
    }

    [Test]
    public async Task ShouldBindAsyncOnOk()
    {
        Result<int> result = 2;
        await result.BindAsync(value => OkAsync(value * 10)).AssertOk(20);
    }

    [Test]
    public async Task ShouldPassErrorThroughBindAsyncWithoutCallingIt()
    {
        Result<int> result = TestError;
        var called = false;

        var bound = result.BindAsync(value =>
        {
            called = true;
            return OkAsync(value);
        });

        await bound.AssertError(TestError);
        await Assert.That(called).IsFalse();
    }

    [Test]
    public async Task ShouldMapErrorAsyncOnError()
    {
        Result<int> result = TestError;
        await result.MapErrorAsync(_ => YieldAsync(OtherError)).AssertError(OtherError);
    }

    [Test]
    public async Task ShouldPassOkThroughMapErrorAsync()
    {
        Result<int> result = 1;
        await result.MapErrorAsync(_ => YieldAsync(OtherError)).AssertOk(1);
    }

    [Test]
    public async Task ShouldKeepOkWhenEnsureAsyncPasses()
    {
        Result<int> result = 5;
        await result.EnsureAsync(value => YieldAsync(value > 0), TestError).AssertOk(5);
    }

    [Test]
    public async Task ShouldFailWhenEnsureAsyncFails()
    {
        Result<int> result = -5;
        await result.EnsureAsync(value => YieldAsync(value > 0), TestError).AssertError(TestError);
    }

    [Test]
    public void ShouldThrowWhenEnsureAsyncGivenNullError()
    {
        Result<int> result = 1;
        Assert.Throws<ArgumentNullException>(() =>
        {
            _ = result.EnsureAsync(value => YieldAsync(value > 0), null!);
        });
    }

    [Test]
    public async Task ShouldTapAsyncOnOk()
    {
        Result<int> result = 1;
        var tapped = 0;

        await result.TapAsync(async value =>
        {
            await Task.Yield();
            tapped = value;
        });

        await Assert.That(tapped).IsEqualTo(1);
    }

    [Test]
    public async Task ShouldNotTapAsyncOnError()
    {
        Result<int> result = TestError;
        var called = false;

        await result.TapAsync(async _ =>
        {
            await Task.Yield();
            called = true;
        });

        await Assert.That(called).IsFalse();
    }

    [Test]
    public async Task ShouldTapErrorAsyncOnError()
    {
        Result<int> result = TestError;
        Error? tapped = null;

        await result.TapErrorAsync(async error =>
        {
            await Task.Yield();
            tapped = error;
        });

        await Assert.That(tapped).IsEqualTo(TestError);
    }

    [Test]
    public async Task ShouldNotTapErrorAsyncOnOk()
    {
        Result<int> result = 1;
        var called = false;

        await result.TapErrorAsync(async _ =>
        {
            await Task.Yield();
            called = true;
        });

        await Assert.That(called).IsFalse();
    }

    [Test]
    public async Task ShouldKeepOkThroughOrElseAsyncWithoutCallingAlternative()
    {
        Result<int> result = 1;
        var called = false;

        var orElsed = result.OrElseAsync(_ =>
        {
            called = true;
            return OkAsync(2);
        });

        await orElsed.AssertOk(1);
        await Assert.That(called).IsFalse();
    }

    [Test]
    public async Task ShouldUseAlternativeFromOrElseAsyncOnError()
    {
        Result<int> result = TestError;
        await result.OrElseAsync(_ => OkAsync(2)).AssertOk(2);
    }

    [Test]
    public async Task ShouldMatchAsyncOnOk()
    {
        Result<int> result = 2;
        var matched = await result.MatchAsync(
            value => YieldAsync(value * 10),
            _ => YieldAsync(-1)
        );
        await Assert.That(matched).IsEqualTo(20);
    }

    [Test]
    public async Task ShouldMatchAsyncOnError()
    {
        Result<int> result = TestError;
        var matched = await result.MatchAsync(
            _ => YieldAsync("ok"),
            error => YieldAsync(error.Code)
        );
        await Assert.That(matched).IsEqualTo(TestError.Code);
    }

    [Test]
    public async Task ShouldMapTaskResult()
    {
        await OkAsync(2).Map(value => value * 10).AssertOk(20);
    }

    [Test]
    public async Task ShouldMapAsyncTaskResult()
    {
        await OkAsync(2).MapAsync(value => YieldAsync(value * 10)).AssertOk(20);
    }

    [Test]
    public async Task ShouldBindTaskResult()
    {
        await OkAsync(2).Bind(value => Result.Ok(value * 10)).AssertOk(20);
    }

    [Test]
    public async Task ShouldBindAsyncTaskResult()
    {
        await OkAsync(2).BindAsync(value => OkAsync(value * 10)).AssertOk(20);
    }

    [Test]
    public async Task ShouldMapErrorTaskResult()
    {
        await FailAsync(TestError).MapError(_ => OtherError).AssertError(OtherError);
    }

    [Test]
    public async Task ShouldMapErrorAsyncTaskResult()
    {
        await FailAsync(TestError)
            .MapErrorAsync(_ => YieldAsync(OtherError))
            .AssertError(OtherError);
    }

    [Test]
    public async Task ShouldEnsureTaskResult()
    {
        await OkAsync(-5).Ensure(value => value > 0, TestError).AssertError(TestError);
    }

    [Test]
    public async Task ShouldEnsureAsyncTaskResult()
    {
        await OkAsync(-5)
            .EnsureAsync(value => YieldAsync(value > 0), TestError)
            .AssertError(TestError);
    }

    [Test]
    public void ShouldThrowWhenEnsureGivenNullErrorOnTaskResult()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            _ = OkAsync(1).Ensure(value => value > 0, null!);
        });
    }

    [Test]
    public void ShouldThrowWhenEnsureAsyncGivenNullErrorOnTaskResult()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            _ = OkAsync(1).EnsureAsync(value => YieldAsync(value > 0), null!);
        });
    }

    [Test]
    public async Task ShouldAwaitAsyncLambdaPassedToTapOnTaskResult()
    {
        Result<int> result = 1;
        var ran = false;

        await Task.FromResult(result)
            .TapAsync(async _ =>
            {
                await Task.Yield();
                ran = true;
            });

        await Assert.That(ran).IsTrue();
    }

    [Test]
    public async Task ShouldTapAsyncTaskResult()
    {
        var tapped = 0;

        await OkAsync(1)
            .TapAsync(async value =>
            {
                await Task.Yield();
                tapped = value;
            });

        await Assert.That(tapped).IsEqualTo(1);
    }

    [Test]
    public async Task ShouldTapErrorAsyncTaskResult()
    {
        Error? tapped = null;

        await FailAsync(TestError)
            .TapErrorAsync(async error =>
            {
                await Task.Yield();
                tapped = error;
            });

        await Assert.That(tapped).IsEqualTo(TestError);
    }

    [Test]
    public async Task ShouldGetValueOrDefaultFromTaskResult()
    {
        await Assert.That(await FailAsync(TestError).GetValueOrDefault(-1)).IsEqualTo(-1);
    }

    [Test]
    public async Task ShouldOrElseTaskResult()
    {
        await FailAsync(TestError).OrElse(_ => Result.Ok(2)).AssertOk(2);
    }

    [Test]
    public async Task ShouldKeepOkThroughTaskResultOrElseWithoutCallingAlternative()
    {
        var called = false;

        var orElsed = OkAsync(1)
            .OrElse(_ =>
            {
                called = true;
                return Result.Ok(2);
            });

        await orElsed.AssertOk(1);
        await Assert.That(called).IsFalse();
    }

    [Test]
    public async Task ShouldOrElseAsyncTaskResult()
    {
        await FailAsync(TestError).OrElseAsync(_ => OkAsync(2)).AssertOk(2);
    }

    [Test]
    public async Task ShouldKeepOkThroughTaskResultOrElseAsyncWithoutCallingAlternative()
    {
        var called = false;

        var orElsed = OkAsync(1)
            .OrElseAsync(_ =>
            {
                called = true;
                return OkAsync(2);
            });

        await orElsed.AssertOk(1);
        await Assert.That(called).IsFalse();
    }

    [Test]
    public async Task ShouldPassUninitializedErrorToOrElseAsyncAlternativeWhenDefault()
    {
        Error? passed = null;

        async Task<Result<int>> DefaultAsync()
        {
            await Task.Yield();
            return default;
        }

        await DefaultAsync()
            .OrElseAsync(error =>
            {
                passed = error;
                return OkAsync(2);
            });

        await Assert.That(passed).IsEqualTo(Result.UninitializedError);
    }

    [Test]
    public async Task ShouldPassUninitializedErrorToMatchAsyncOnErrorWhenDefault()
    {
        Result<int> result = default;
        var matched = await result.MatchAsync(
            value => YieldAsync(value.ToString()),
            error => YieldAsync(error.Code)
        );
        await Assert.That(matched).IsEqualTo(Result.UninitializedError.Code);
    }

    [Test]
    public async Task ShouldMatchTaskResultOnOk()
    {
        var matched = await OkAsync(2).Match(value => value * 10, _ => -1);
        await Assert.That(matched).IsEqualTo(20);
    }

    [Test]
    public async Task ShouldMatchTaskResultOnError()
    {
        var matched = await FailAsync(TestError).Match(_ => "ok", error => error.Code);
        await Assert.That(matched).IsEqualTo(TestError.Code);
    }

    [Test]
    public async Task ShouldMatchAsyncTaskResultOnOk()
    {
        var matched = await OkAsync(2)
            .MatchAsync(value => YieldAsync(value * 10), _ => YieldAsync(-1));
        await Assert.That(matched).IsEqualTo(20);
    }

    [Test]
    public async Task ShouldMatchAsyncTaskResultOnError()
    {
        var matched = await FailAsync(TestError)
            .MatchAsync(_ => YieldAsync("ok"), error => YieldAsync(error.Code));
        await Assert.That(matched).IsEqualTo(TestError.Code);
    }

    [Test]
    public async Task ShouldConvertTaskResultToOptional()
    {
        await OkAsync(1).ToOptional().AssertSome(1);
    }

    [Test]
    public async Task ShouldConvertTaskResultToUnit()
    {
        await OkAsync(1).ToUnit().AssertOk(Unit.Default);
    }

    [Test]
    public async Task ShouldKeepErrorThroughTaskResultToUnit()
    {
        await FailAsync(TestError).ToUnit().AssertError(TestError);
    }

    [Test]
    public async Task ShouldChainSyncAndAsyncStepsOnTaskResult()
    {
        var result = OkAsync(2)
            .BindAsync(value => OkAsync(value * 10))
            .Map(value => value + 1)
            .EnsureAsync(value => YieldAsync(value > 0), TestError);

        await result.AssertOk(21);
    }

    [Test]
    public async Task ShouldShortCircuitChainOnFirstError()
    {
        var called = false;

        var result = OkAsync(2)
            .BindAsync(_ => FailAsync(TestError))
            .Map(value =>
            {
                called = true;
                return value;
            });

        await result.AssertError(TestError);
        await Assert.That(called).IsFalse();
    }
}
