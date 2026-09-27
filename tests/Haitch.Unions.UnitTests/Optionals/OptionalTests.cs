using Haitch.Unions.TUnit.Assertions;

namespace Haitch.Unions.UnitTests.Optionals;

public class OptionalTests
{
    [Test]
    public async Task ShouldBeSomeFromImplicitConversion()
    {
        Optional<int> optional = 1;
        await optional.AssertSome(1);
    }

    [Test]
    public async Task ShouldBeSomeFromSomeFactory()
    {
        var optional = Optional<string>.Some("value");
        await optional.AssertSome("value");
    }

    [Test]
    public async Task ShouldThrowWhenSomeGivenNull()
    {
        await Assert.That(() => Optional<string>.Some(null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ShouldBeSomeFromFromWhenNotNull()
    {
        var optional = Optional<string>.From("value");
        await optional.AssertSome("value");
    }

    [Test]
    public async Task ShouldBeNoneFromFromWhenNull()
    {
        var optional = Optional<string>.From(null);
        await optional.AssertNone();
    }

    [Test]
    public async Task ShouldBeSomeFromOptionalSome()
    {
        var optional = Optional.Some("value");
        await optional.AssertSome("value");
    }

    [Test]
    public async Task ShouldThrowWhenOptionalSomeGivenNull()
    {
        await Assert.That(() => Optional.Some<string>(null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ShouldBeSomeFromOptionalFromWhenNotNull()
    {
        var optional = Optional.From("value");
        await optional.AssertSome("value");
    }

    [Test]
    public async Task ShouldBeNoneFromOptionalFromWhenNull()
    {
        string? value = null;
        var optional = Optional.From(value);
        await optional.AssertNone();
    }

    [Test]
    public async Task ShouldBeSomeFromOptionalFromWhenNonNullableStruct()
    {
        // 5 has an identity conversion to int, preferred over the implicit conversion
        // to int? needed by the nullable-struct overload, so this binds to the
        // notnull overload (Optional<T> From<T>(T? value) where T : notnull).
        Optional<int> optional = Optional.From(5);
        await optional.AssertSome(5);
    }

    [Test]
    public async Task ShouldBeSomeFromOptionalFromWhenNullableStructHasValue()
    {
        int? value = 1;
        var optional = Optional.From(value);
        await optional.AssertSome(1);
    }

    [Test]
    public async Task ShouldBeNoneFromOptionalFromWhenNullableStructIsNull()
    {
        int? value = null;
        var optional = Optional.From(value);
        await optional.AssertNone();
    }

    [Test]
    public async Task ShouldBeNoneFromOptionalNone()
    {
        Optional<int> optional = Optional.None<int>();
        await optional.AssertNone();
    }

    [Test]
    public async Task ShouldBeNoneFromNoneFactory()
    {
        var optional = Optional<int>.None();
        await optional.AssertNone();
    }

    [Test]
    public async Task ShouldBeNoneFromNoneConversion()
    {
        Optional<int> optional = default(None);
        await optional.AssertNone();
    }

    [Test]
    public async Task ShouldBeSomeFromValueConstructor()
    {
        var optional = new Optional<string>("value");
        await optional.AssertSome("value");
    }

    [Test]
    public async Task ShouldBeNoneFromValueConstructorWhenNull()
    {
        var optional = new Optional<string>(null);
        await optional.AssertNone();
    }

    [Test]
    public async Task ShouldBeNoneFromNoneConstructor()
    {
        var optional = new Optional<int>(default(None));
        await optional.AssertNone();
    }

    [Test]
    public async Task ShouldBeNoneFromNullReference()
    {
        Optional<string> optional = null;
        await optional.AssertNone();
    }

    [Test]
    public async Task ShouldBeNoneWhenDefault()
    {
        Optional<int> optional = default;
        await optional.AssertNone();
    }

    [Test]
    public async Task ShouldBeNoneWhenParameterless()
    {
        var optional = new Optional<int>();
        await optional.AssertNone();
    }

    [Test]
    public async Task ShouldExposeValueAsUnionValueWhenSome()
    {
        Optional<int> optional = 1;
        await Assert.That(optional.Value).IsEqualTo(1);
    }

    [Test]
    public async Task ShouldExposeNoneAsUnionValueWhenNone()
    {
        var optional = Optional<int>.None();
        await Assert.That(optional.Value).IsEqualTo(default(None));
    }

    [Test]
    public async Task ShouldExposeNoneAsUnionValueWhenDefault()
    {
        Optional<int> optional = default;
        await Assert.That(optional.Value).IsEqualTo(default(None));
    }

    [Test]
    public async Task ShouldGetValueWhenSome()
    {
        Optional<int> optional = 1;
        var hasValue = optional.TryGetValue(out int value);
        await Assert.That(hasValue).IsTrue();
        await Assert.That(value).IsEqualTo(1);
    }

    [Test]
    public async Task ShouldNotGetValueWhenNone()
    {
        var optional = Optional<int>.None();
        var hasValue = optional.TryGetValue(out int _);
        await Assert.That(hasValue).IsFalse();
    }

    [Test]
    public async Task ShouldGetNoneWhenNone()
    {
        var optional = Optional<int>.None();
        var isNone = optional.TryGetValue(out None _);
        await Assert.That(isNone).IsTrue();
    }

    [Test]
    public async Task ShouldNotGetNoneWhenSome()
    {
        Optional<int> optional = 1;
        var isNone = optional.TryGetValue(out None _);
        await Assert.That(isNone).IsFalse();
    }

    [Test]
    public async Task ShouldMatchSome()
    {
        Optional<int> optional = 2;
        var matched = optional.Match(value => value * 10, () => -1);
        await Assert.That(matched).IsEqualTo(20);
    }

    [Test]
    public async Task ShouldMatchNone()
    {
        var optional = Optional<int>.None();
        var matched = optional.Match(value => value * 10, () => -1);
        await Assert.That(matched).IsEqualTo(-1);
    }

    [Test]
    public async Task ShouldMatchNoneWhenDefault()
    {
        Optional<string> optional = default;
        var matched = optional.Match(value => value, () => "none");
        await Assert.That(matched).IsEqualTo("none");
    }

    [Test]
    public async Task ShouldEqualSomeWithSameValue()
    {
        Optional<int> first = 1;
        Optional<int> second = 1;

        await Assert.That(first == second).IsTrue();
        await Assert.That(first.Equals((object)second)).IsTrue();
        await Assert.That(first.GetHashCode()).IsEqualTo(second.GetHashCode());
    }

    [Test]
    public async Task ShouldNotEqualSomeWithDifferentValue()
    {
        Optional<int> first = 1;
        Optional<int> second = 2;
        await Assert.That(first != second).IsTrue();
    }

    [Test]
    public async Task ShouldEqualNoneAndDefault()
    {
        var none = Optional<int>.None();
        Optional<int> fromDefault = default;

        await Assert.That(none == fromDefault).IsTrue();
        await Assert.That(none.GetHashCode()).IsEqualTo(fromDefault.GetHashCode());
    }

    [Test]
    public async Task ShouldNotEqualSomeAndNone()
    {
        Optional<int> some = 0;
        await Assert.That(some != Optional<int>.None()).IsTrue();
    }

    [Test]
    public async Task ShouldFormatSomeAsString()
    {
        Optional<int> optional = 42;
        await Assert.That(optional.ToString()).IsEqualTo("Some(42)");
    }

    [Test]
    public async Task ShouldFormatNoneAsString()
    {
        await Assert.That(Optional<int>.None().ToString()).IsEqualTo("None");
    }
}
