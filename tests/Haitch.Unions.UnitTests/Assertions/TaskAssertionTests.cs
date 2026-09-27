using Haitch.Unions.TUnit.Assertions;
using TUnit.Assertions.Exceptions;

namespace Haitch.Unions.UnitTests.Assertions;

public class TaskAssertionTests
{
    private record UserNotFoundError() : NotFoundError("User.NotFound", "No user with that id");

    private static readonly Error TestError = new("Test.Error", "Something went wrong");

    private static async Task<Result<int>> ResultAsync(Result<int> result)
    {
        await Task.Yield();
        return result;
    }

    private static async Task<Optional<int>> OptionalAsync(Optional<int> optional)
    {
        await Task.Yield();
        return optional;
    }

    public static IEnumerable<
        Func<(Error Error, Func<Task<Result<int>>, Task> Assertion)>
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
    public async Task ShouldPassWhenTaskErrorKindMatches(
        (Error Error, Func<Task<Result<int>>, Task> Assertion) testCase
    )
    {
        await testCase.Assertion(ResultAsync(testCase.Error));
    }

    [Test]
    public async Task ShouldFailAssertOkWhenTaskIsError()
    {
        await Assert.That(() => ResultAsync(TestError).AssertOk()).Throws<AssertionException>();
    }

    [Test]
    public async Task ShouldFailAssertOkWhenTaskValueDiffers()
    {
        await Assert.That(() => ResultAsync(1).AssertOk(2)).Throws<AssertionException>();
    }

    [Test]
    public async Task ShouldFailAssertErrorWhenTaskIsOk()
    {
        await Assert.That(() => ResultAsync(1).AssertError()).Throws<AssertionException>();
    }

    [Test]
    public async Task ShouldFailAssertErrorWhenTaskErrorDiffers()
    {
        var other = new Error("Other.Error", "Something else went wrong");
        await Assert
            .That(() => ResultAsync(TestError).AssertError(other))
            .Throws<AssertionException>();
    }

    [Test]
    public async Task ShouldFailAssertNotFoundWhenTaskErrorKindDiffers()
    {
        var result = ResultAsync(new ConflictError("Code", "Description"));
        await Assert
            .That(async () =>
            {
                await result.AssertNotFound();
            })
            .Throws<AssertionException>();
    }

    [Test]
    public async Task ShouldPassAssertErrorForTask()
    {
        await ResultAsync(new NotFoundError("Code", "Description"))
            .AssertError<int, NotFoundError>();
    }

    [Test]
    public async Task ShouldReturnTypedErrorFromAssertErrorForTask()
    {
        var failures = new Dictionary<string, string[]> { ["field"] = ["required"] };
        var error = await ResultAsync(
            new ValidationError("Code", "Description", failures)
        ).AssertError<int, ValidationError>();
        await Assert.That(error.Failures).IsEqualTo(failures);
    }

    [Test]
    public async Task ShouldReturnTypedErrorFromPerKindHelperForTask()
    {
        var failures = new Dictionary<string, string[]> { ["field"] = ["required"] };
        var error = await ResultAsync(
            new ValidationError("Code", "Description", failures)
        ).AssertValidation();
        await Assert.That(error.Failures).IsEqualTo(failures);
    }

    [Test]
    public async Task ShouldFailAssertErrorForTaskWhenErrorIsParentType()
    {
        var result = ResultAsync(new NotFoundError("Code", "Description"));
        await Assert
            .That(async () =>
            {
                await result.AssertError<int, UserNotFoundError>();
            })
            .Throws<AssertionException>();
    }

    [Test]
    public async Task ShouldFailAssertErrorForTaskWhenOk()
    {
        await Assert
            .That(async () =>
            {
                await ResultAsync(1).AssertError<int, NotFoundError>();
            })
            .Throws<AssertionException>();
    }

    [Test]
    public async Task ShouldFailAssertSomeWhenTaskIsNone()
    {
        await Assert
            .That(() => OptionalAsync(Optional<int>.None()).AssertSome())
            .Throws<AssertionException>();
    }

    [Test]
    public async Task ShouldFailAssertSomeWhenTaskValueDiffers()
    {
        await Assert.That(() => OptionalAsync(1).AssertSome(2)).Throws<AssertionException>();
    }

    [Test]
    public async Task ShouldFailAssertNoneWhenTaskIsSome()
    {
        await Assert.That(() => OptionalAsync(1).AssertNone()).Throws<AssertionException>();
    }
}
