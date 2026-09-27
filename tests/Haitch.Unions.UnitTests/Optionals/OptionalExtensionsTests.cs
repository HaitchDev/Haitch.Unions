using Haitch.Unions.TUnit.Assertions;

namespace Haitch.Unions.UnitTests.Optionals;

public class OptionalExtensionsTests
{
    private static readonly Error TestError = new("Test.Error", "Something went wrong");

    [Test]
    public async Task ShouldMapValueWhenSome()
    {
        Optional<int> optional = 2;
        await optional.Map(value => value.ToString()).AssertSome("2");
    }

    [Test]
    public async Task ShouldStayNoneThroughMapWithoutCallingIt()
    {
        var called = false;

        var mapped = Optional<int>
            .None()
            .Map(value =>
            {
                called = true;
                return value;
            });

        await mapped.AssertNone();
        await Assert.That(called).IsFalse();
    }

    [Test]
    public async Task ShouldBindToSomeWhenSome()
    {
        Optional<int> optional = 2;
        await optional.Bind(value => Optional.Some(value * 10)).AssertSome(20);
    }

    [Test]
    public async Task ShouldBindToNoneWhenSome()
    {
        Optional<int> optional = 2;
        await optional.Bind(_ => Optional.None<int>()).AssertNone();
    }

    [Test]
    public async Task ShouldStayNoneThroughBind()
    {
        await Optional<int>.None().Bind(value => Optional.Some(value)).AssertNone();
    }

    [Test]
    public async Task ShouldKeepValueWhenEnsurePasses()
    {
        Optional<int> optional = 5;
        await optional.Ensure(value => value > 0).AssertSome(5);
    }

    [Test]
    public async Task ShouldBecomeNoneWhenEnsureFails()
    {
        Optional<int> optional = -5;
        await optional.Ensure(value => value > 0).AssertNone();
    }

    [Test]
    public async Task ShouldTapValueWhenSome()
    {
        Optional<int> optional = 1;
        var tapped = 0;

        var returned = optional.Tap(value => tapped = value);

        await Assert.That(tapped).IsEqualTo(1);
        await returned.AssertSome(1);
    }

    [Test]
    public async Task ShouldNotTapWhenNone()
    {
        var called = false;
        Optional<int>.None().Tap(_ => called = true);
        await Assert.That(called).IsFalse();
    }

    [Test]
    public async Task ShouldTapNoneWhenNone()
    {
        var called = false;

        var returned = Optional<int>.None().TapNone(() => called = true);

        await Assert.That(called).IsTrue();
        await returned.AssertNone();
    }

    [Test]
    public async Task ShouldNotTapNoneWhenSome()
    {
        var called = false;
        Optional<int> optional = 1;

        optional.TapNone(() => called = true);

        await Assert.That(called).IsFalse();
    }

    [Test]
    public async Task ShouldGetValueWhenSome()
    {
        Optional<int> optional = 1;
        await Assert.That(optional.GetValueOrDefault(-1)).IsEqualTo(1);
    }

    [Test]
    public async Task ShouldGetFallbackWhenNone()
    {
        await Assert.That(Optional<int>.None().GetValueOrDefault(-1)).IsEqualTo(-1);
    }

    [Test]
    public async Task ShouldKeepValueThroughOrElseWithoutCallingAlternative()
    {
        Optional<int> optional = 1;
        var called = false;

        var result = optional.OrElse(() =>
        {
            called = true;
            return Optional.Some(2);
        });

        await result.AssertSome(1);
        await Assert.That(called).IsFalse();
    }

    [Test]
    public async Task ShouldUseAlternativeFromOrElseWhenNone()
    {
        await Optional<int>.None().OrElse(() => Optional.Some(2)).AssertSome(2);
    }

    [Test]
    public async Task ShouldConvertSomeToOk()
    {
        Optional<int> optional = 1;
        await optional.ToResult(TestError).AssertOk(1);
    }

    [Test]
    public async Task ShouldConvertNoneToError()
    {
        await Optional<int>.None().ToResult(TestError).AssertError(TestError);
    }

    [Test]
    public async Task ShouldThrowWhenToResultGivenNullError()
    {
        Optional<int> optional = 1;
        await Assert.That(() => optional.ToResult(null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ShouldConvertSomeToNullable()
    {
        Optional<int> optional = 1;
        await Assert.That(optional.ToNullable()).IsEqualTo(1);
    }

    [Test]
    public async Task ShouldConvertNoneToNull()
    {
        await Assert.That(Optional<int>.None().ToNullable()).IsNull();
    }

    [Test]
    public async Task ShouldConvertSomeReferenceTypeToNullable()
    {
        Optional<string> optional = "value";
        await Assert.That(optional.ToNullable()).IsEqualTo("value");
    }

    [Test]
    public async Task ShouldConvertNoneReferenceTypeToNull()
    {
        await Assert.That(Optional<string>.None().ToNullable()).IsNull();
    }
}
