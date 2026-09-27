using Haitch.Unions.TUnit.Assertions;
using TUnit.Assertions.Exceptions;

namespace Haitch.Unions.UnitTests.Assertions;

public class ErrorAssertionTests
{
    private record UserNotFoundError() : NotFoundError("User.NotFound", "No user with that id");

    public static IEnumerable<
        Func<(Error Error, Func<Result<int>, Task> Assertion)>
    > MatchingAssertions()
    {
        yield return () => (new NotFoundError("Code", "Description"), r => r.AssertNotFound());
        yield return () =>
        (
            new ValidationError("Code", "Description", new Dictionary<string, string[]>()),
            r => r.AssertValidation()
        );
        yield return () => (new ConflictError("Code", "Description"), r => r.AssertConflict());
        yield return () =>
            (new UnauthorizedError("Code", "Description"), r => r.AssertUnauthorized());
        yield return () => (new ForbiddenError("Code", "Description"), r => r.AssertForbidden());
        yield return () => (new UnexpectedError("Code", "Description"), r => r.AssertUnexpected());
    }

    [Test]
    [MethodDataSource(nameof(MatchingAssertions))]
    public async Task ShouldPassWhenErrorKindMatches(
        (Error Error, Func<Result<int>, Task> Assertion) testCase
    )
    {
        Result<int> result = testCase.Error;
        await testCase.Assertion(result);
    }

    [Test]
    public async Task ShouldPassWhenErrorDerivesFromKind()
    {
        Result<int> result = new UserNotFoundError();
        await result.AssertNotFound();
    }

    [Test]
    public async Task ShouldPassAssertErrorForCustomErrorType()
    {
        Result<int> result = new UserNotFoundError();
        await result.AssertError<int, UserNotFoundError>();
    }

    [Test]
    public async Task ShouldReturnTypedErrorFromAssertError()
    {
        var failures = new Dictionary<string, string[]> { ["field"] = ["required"] };
        Result<int> result = new ValidationError("Code", "Description", failures);
        var error = await result.AssertError<int, ValidationError>();
        await Assert.That(error.Failures).IsEqualTo(failures);
    }

    [Test]
    public async Task ShouldReturnTypedErrorFromPerKindHelper()
    {
        var failures = new Dictionary<string, string[]> { ["field"] = ["required"] };
        Result<int> result = new ValidationError("Code", "Description", failures);
        var error = await result.AssertValidation();
        await Assert.That(error.Failures).IsEqualTo(failures);
    }

    [Test]
    public async Task ShouldFailAssertErrorWhenErrorIsParentType()
    {
        Result<int> result = new NotFoundError("Code", "Description");
        await Assert
            .That(async () =>
            {
                await result.AssertError<int, UserNotFoundError>();
            })
            .Throws<AssertionException>();
    }

    [Test]
    public async Task ShouldFailAssertErrorWhenResultIsOk()
    {
        Result<int> result = 1;
        await Assert
            .That(async () =>
            {
                await result.AssertError<int, NotFoundError>();
            })
            .Throws<AssertionException>();
    }

    [Test]
    public async Task ShouldFailWhenErrorKindDiffers()
    {
        Result<int> result = new ConflictError("Code", "Description");
        await Assert
            .That(async () =>
            {
                await result.AssertNotFound();
            })
            .Throws<AssertionException>();
    }

    [Test]
    public async Task ShouldFailWhenErrorIsBaseError()
    {
        Result<int> result = new Error("Code", "Description");
        await Assert
            .That(async () =>
            {
                await result.AssertNotFound();
            })
            .Throws<AssertionException>();
    }

    [Test]
    public async Task ShouldFailWhenResultIsOk()
    {
        Result<int> result = 1;
        await Assert
            .That(async () =>
            {
                await result.AssertNotFound();
            })
            .Throws<AssertionException>();
    }

    [Test]
    public async Task ShouldIncludeActualErrorCodeInAssertOkFailureMessage()
    {
        Result<int> result = new NotFoundError("Some.Distinctive.Code", "Description");
        await Assert
            .That(async () =>
            {
                await result.AssertOk();
            })
            .Throws<AssertionException>()
            .WithMessageContaining("Some.Distinctive.Code");
    }
}
