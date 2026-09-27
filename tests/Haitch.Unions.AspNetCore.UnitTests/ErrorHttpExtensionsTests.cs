using Microsoft.AspNetCore.Http;

namespace Haitch.Unions.AspNetCore.UnitTests;

public class ErrorHttpExtensionsTests
{
    private record RateLimitedError() : Error("Rate.Limited", "Too many requests"), IHttpError
    {
        public int StatusCode => 429;
    }

    private record GoneError() : NotFoundError("User.Gone", "The user was deleted"), IHttpError
    {
        public int StatusCode => 410;
    }

    public static IEnumerable<Func<(Error Error, int StatusCode)>> StatusCodes()
    {
        yield return () => (new NotFoundError("Code", "Description"), 404);
        yield return () =>
            (new ValidationError("Code", "Description", new Dictionary<string, string[]>()), 400);
        yield return () => (new ConflictError("Code", "Description"), 409);
        yield return () => (new UnauthorizedError("Code", "Description"), 401);
        yield return () => (new ForbiddenError("Code", "Description"), 403);
        yield return () => (new UnexpectedError("Code", "Description"), 500);
        yield return () => (new Error("Code", "Description"), 400);
    }

    [Test]
    [MethodDataSource(nameof(StatusCodes))]
    public async Task ShouldMapErrorToStatusCode((Error Error, int StatusCode) testCase)
    {
        await Assert.That(testCase.Error.ToStatusCode()).IsEqualTo(testCase.StatusCode);
    }

    [Test]
    public async Task ShouldUseStatusCodeFromHttpError()
    {
        await Assert.That(new RateLimitedError().ToStatusCode()).IsEqualTo(429);
    }

    [Test]
    public async Task ShouldPreferHttpErrorStatusCodeOverKind()
    {
        await Assert.That(new GoneError().ToStatusCode()).IsEqualTo(410);
    }

    [Test]
    public async Task ShouldUseHttpErrorStatusCodeInProblemDetails()
    {
        await Assert.That(new RateLimitedError().ToProblemDetails().Status).IsEqualTo(429);
    }

    [Test]
    public async Task ShouldCreateProblemDetailsFromError()
    {
        var problem = new NotFoundError("User.NotFound", "No user with that id").ToProblemDetails();

        await Assert.That(problem.Status).IsEqualTo(404);
        await Assert.That(problem.Detail).IsEqualTo("No user with that id");
        await Assert.That(problem.Extensions["code"]).IsEqualTo("User.NotFound");
    }

    [Test]
    public async Task ShouldCreateValidationProblemDetailsFromValidationError()
    {
        var error = new ValidationError(
            "Validation.Failed",
            "One or more fields are invalid",
            new Dictionary<string, string[]> { ["Name"] = ["Required"] }
        );

        var problem = error.ToProblemDetails();

        await Assert.That(problem).IsTypeOf<HttpValidationProblemDetails>();
        var validation = (HttpValidationProblemDetails)problem;
        await Assert.That(validation.Status).IsEqualTo(400);
        await Assert.That(validation.Detail).IsEqualTo("One or more fields are invalid");
        await Assert.That(validation.Extensions["code"]).IsEqualTo("Validation.Failed");
        await Assert.That(validation.Errors["Name"]).IsEquivalentTo(["Required"]);
    }
}
