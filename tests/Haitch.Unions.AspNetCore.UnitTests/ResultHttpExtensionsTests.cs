using Microsoft.AspNetCore.Http.HttpResults;

namespace Haitch.Unions.AspNetCore.UnitTests;

public class ResultHttpExtensionsTests
{
    [Test]
    public async Task ShouldReturnOkWithValueOnOk()
    {
        Result<int> result = 42;

        var httpResult = result.ToHttpResult();

        await Assert.That(httpResult).IsTypeOf<Ok<int>>();
        await Assert.That(((Ok<int>)httpResult).Value).IsEqualTo(42);
    }

    [Test]
    public async Task ShouldReturnNoContentOnUnitOk()
    {
        var result = Result.Ok();

        var httpResult = result.ToHttpResult();

        await Assert.That(httpResult).IsTypeOf<NoContent>();
    }

    [Test]
    public async Task ShouldReturnProblemOnError()
    {
        Result<int> result = new NotFoundError("User.NotFound", "No user with that id");

        var httpResult = result.ToHttpResult();

        await Assert.That(httpResult).IsTypeOf<ProblemHttpResult>();
        var problem = (ProblemHttpResult)httpResult;
        await Assert.That(problem.StatusCode).IsEqualTo(404);
        await Assert.That(problem.ProblemDetails.Detail).IsEqualTo("No user with that id");
    }

    [Test]
    public async Task ShouldReturnProblemOnUnitError()
    {
        var result = Result.Error(
            new ConflictError("User.Exists", "A user with that email exists")
        );

        var httpResult = result.ToHttpResult();

        await Assert.That(httpResult).IsTypeOf<ProblemHttpResult>();
        await Assert.That(((ProblemHttpResult)httpResult).StatusCode).IsEqualTo(409);
    }

    [Test]
    public async Task ShouldReturnOkFromTaskOnOk()
    {
        var httpResult = await ResultAsync(Result.Ok(42)).ToHttpResultAsync();

        await Assert.That(httpResult).IsTypeOf<Ok<int>>();
        await Assert.That(((Ok<int>)httpResult).Value).IsEqualTo(42);
    }

    [Test]
    public async Task ShouldReturnNoContentFromTaskOnUnitOk()
    {
        var httpResult = await ResultAsync(Result.Ok()).ToHttpResultAsync();
        await Assert.That(httpResult).IsTypeOf<NoContent>();
    }

    [Test]
    public async Task ShouldReturnProblemFromTaskOnError()
    {
        var httpResult = await ResultAsync(
                Result.Error<int>(new NotFoundError("User.NotFound", "No user"))
            )
            .ToHttpResultAsync();

        await Assert.That(httpResult).IsTypeOf<ProblemHttpResult>();
        await Assert.That(((ProblemHttpResult)httpResult).StatusCode).IsEqualTo(404);
    }

    private static async Task<Result<T>> ResultAsync<T>(Result<T> result)
        where T : notnull
    {
        await Task.Yield();
        return result;
    }
}
