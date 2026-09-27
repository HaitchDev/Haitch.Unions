using Haitch.Unions.TUnit.Assertions;

namespace Haitch.Unions.UnitTests.Results;

public class ResultExtensionsTests
{
    private static readonly Error TestError = new("Test.Error", "Something went wrong");
    private static readonly Error OtherError = new("Other.Error", "Something else went wrong");

    [Test]
    public async Task ShouldMapValueOnOk()
    {
        Result<int> result = 2;
        await result.Map(value => value.ToString()).AssertOk("2");
    }

    [Test]
    public async Task ShouldPassErrorThroughMapWithoutCallingIt()
    {
        Result<int> result = TestError;
        var called = false;

        var mapped = result.Map(value =>
        {
            called = true;
            return value;
        });

        await mapped.AssertError(TestError);
        await Assert.That(called).IsFalse();
    }

    [Test]
    public async Task ShouldPassUninitializedErrorThroughMap()
    {
        Result<int> result = default;
        await result.Map(value => value).AssertError(Result.UninitializedError);
    }

    [Test]
    public async Task ShouldBindToOkOnOk()
    {
        Result<int> result = 2;
        await result.Bind(value => Result.Ok(value * 10)).AssertOk(20);
    }

    [Test]
    public async Task ShouldBindToErrorOnOk()
    {
        Result<int> result = 2;
        await result.Bind(_ => Result.Error<int>(TestError)).AssertError(TestError);
    }

    [Test]
    public async Task ShouldPassErrorThroughBindWithoutCallingIt()
    {
        Result<int> result = TestError;
        var called = false;

        var bound = result.Bind(value =>
        {
            called = true;
            return Result.Ok(value);
        });

        await bound.AssertError(TestError);
        await Assert.That(called).IsFalse();
    }

    [Test]
    public async Task ShouldMapErrorOnError()
    {
        Result<int> result = TestError;
        await result.MapError(_ => OtherError).AssertError(OtherError);
    }

    [Test]
    public async Task ShouldPassOkThroughMapError()
    {
        Result<int> result = 1;
        await result.MapError(_ => OtherError).AssertOk(1);
    }

    [Test]
    public async Task ShouldKeepOkWhenEnsurePasses()
    {
        Result<int> result = 5;
        await result.Ensure(value => value > 0, TestError).AssertOk(5);
    }

    [Test]
    public async Task ShouldFailWhenEnsureFails()
    {
        Result<int> result = -5;
        await result.Ensure(value => value > 0, TestError).AssertError(TestError);
    }

    [Test]
    public async Task ShouldKeepOriginalErrorThroughEnsure()
    {
        Result<int> result = OtherError;
        await result.Ensure(value => value > 0, TestError).AssertError(OtherError);
    }

    [Test]
    public async Task ShouldThrowWhenEnsureGivenNullError()
    {
        Result<int> result = 1;
        await Assert
            .That(() => result.Ensure(value => value > 0, null!))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ShouldTapValueOnOk()
    {
        Result<int> result = 1;
        var tapped = 0;

        var returned = result.Tap(value => tapped = value);

        await Assert.That(tapped).IsEqualTo(1);
        await returned.AssertOk(1);
    }

    [Test]
    public async Task ShouldNotTapOnError()
    {
        Result<int> result = TestError;
        var called = false;

        result.Tap(_ => called = true);

        await Assert.That(called).IsFalse();
    }

    [Test]
    public async Task ShouldTapErrorOnError()
    {
        Result<int> result = TestError;
        Error? tapped = null;

        var returned = result.TapError(error => tapped = error);

        await Assert.That(tapped).IsEqualTo(TestError);
        await returned.AssertError(TestError);
    }

    [Test]
    public async Task ShouldNotTapErrorOnOk()
    {
        Result<int> result = 1;
        var called = false;

        result.TapError(_ => called = true);

        await Assert.That(called).IsFalse();
    }

    [Test]
    public async Task ShouldGetValueOnOk()
    {
        Result<int> result = 1;
        await Assert.That(result.GetValueOrDefault(-1)).IsEqualTo(1);
    }

    [Test]
    public async Task ShouldGetFallbackOnError()
    {
        Result<int> result = TestError;
        await Assert.That(result.GetValueOrDefault(-1)).IsEqualTo(-1);
    }

    [Test]
    public async Task ShouldKeepOkThroughOrElseWithoutCallingAlternative()
    {
        Result<int> result = 1;
        var called = false;

        var orElsed = result.OrElse(_ =>
        {
            called = true;
            return Result.Ok(2);
        });

        await orElsed.AssertOk(1);
        await Assert.That(called).IsFalse();
    }

    [Test]
    public async Task ShouldUseAlternativeFromOrElseOnError()
    {
        Result<int> result = TestError;
        Error? received = null;

        var orElsed = result.OrElse(error =>
        {
            received = error;
            return Result.Ok(2);
        });

        await orElsed.AssertOk(2);
        await Assert.That(received).IsEqualTo(TestError);
    }

    [Test]
    public async Task ShouldReturnErrorFromOrElseAlternative()
    {
        Result<int> result = TestError;
        await result.OrElse(_ => Result.Error<int>(OtherError)).AssertError(OtherError);
    }

    [Test]
    public async Task ShouldPassUninitializedErrorThroughOrElse()
    {
        Result<int> result = default;
        Error? received = null;

        result.OrElse(error =>
        {
            received = error;
            return Result.Ok(1);
        });

        await Assert.That(received).IsEqualTo(Result.UninitializedError);
    }

    [Test]
    public async Task ShouldConvertOkToSome()
    {
        Result<int> result = 1;
        await result.ToOptional().AssertSome(1);
    }

    [Test]
    public async Task ShouldConvertErrorToNone()
    {
        Result<int> result = TestError;
        await result.ToOptional().AssertNone();
    }

    [Test]
    public async Task ShouldConvertOkToUnit()
    {
        Result<int> result = 1;
        await result.ToUnit().AssertOk(Unit.Default);
    }

    [Test]
    public async Task ShouldKeepErrorThroughToUnit()
    {
        Result<int> result = TestError;
        await result.ToUnit().AssertError(TestError);
    }

    [Test]
    public async Task ShouldReturnUninitializedErrorThroughToUnitWhenDefault()
    {
        Result<int> result = default;
        await result.ToUnit().AssertError(Result.UninitializedError);
    }
}
