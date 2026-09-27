using Haitch.Unions.TUnit.Assertions;

namespace Haitch.Unions.UnitTests.Results;

public class ResultTests
{
    private static readonly Error TestError = new("Test.Error", "Something went wrong");
    private static readonly Error OtherError = new("Other.Error", "Something else went wrong");

    [Test]
    public async Task ShouldReturnOk()
    {
        Result<int> result = 1;
        await result.AssertOk(1);
    }

    [Test]
    public async Task ShouldReturnOkFromOk()
    {
        var result = Result<string>.Ok("value");
        await result.AssertOk("value");
    }

    [Test]
    public async Task ShouldReturnErrorFromImplicitConversion()
    {
        Result<int> result = TestError;
        await result.AssertError(TestError);
    }

    [Test]
    public async Task ShouldReturnErrorFromErrorFactory()
    {
        var result = Result<int>.Error(TestError);
        await result.AssertError(TestError);
    }

    [Test]
    public async Task ShouldReturnOkFromValueConstructor()
    {
        var result = new Result<int>(1);
        await result.AssertOk(1);
    }

    [Test]
    public async Task ShouldReturnErrorFromErrorConstructor()
    {
        var result = new Result<int>(TestError);
        await result.AssertError(TestError);
    }

    [Test]
    public async Task ShouldReturnOkFromResultOk()
    {
        var result = Result.Ok(1);
        await result.AssertOk(1);
    }

    [Test]
    public async Task ShouldReturnUnitOkFromResultOkWithoutValue()
    {
        var result = Result.Ok();
        await result.AssertOk(default(Unit));
    }

    [Test]
    public async Task ShouldReturnErrorFromResultError()
    {
        var result = Result.Error<int>(TestError);
        await result.AssertError(TestError);
    }

    [Test]
    public async Task ShouldReturnUnitErrorFromResultErrorWithoutType()
    {
        var result = Result.Error(TestError);
        await result.AssertError(TestError);
    }

    [Test]
    public async Task ShouldThrowWhenResultErrorGivenNull()
    {
        await Assert.That(() => Result.Error<int>(null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ShouldThrowWhenConstructedWithNullValue()
    {
        await Assert.That(() => new Result<string>((string)null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ShouldThrowWhenOkGivenNull()
    {
        await Assert.That(() => Result<string>.Ok(null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ShouldThrowWhenResultOkGivenNull()
    {
        await Assert.That(() => Result.Ok<string>(null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ShouldThrowWhenImplicitlyConvertedFromNullValue()
    {
        await Assert
            .That(() =>
            {
                Result<string> result = (string)null!;
                return result;
            })
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ShouldThrowWhenConstructedWithNullError()
    {
        await Assert.That(() => new Result<int>(null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ShouldReturnUninitializedErrorWhenDefault()
    {
        Result<int> result = default;
        await result.AssertError(Result.UninitializedError);
    }

    [Test]
    public async Task ShouldReturnUninitializedErrorWhenParameterless()
    {
        var result = new Result<int>();
        await result.AssertError(Result.UninitializedError);
    }

    [Test]
    public async Task ShouldExposeValueAsUnionValueWhenOk()
    {
        Result<int> result = 1;
        await Assert.That(result.Value).IsEqualTo(1);
    }

    [Test]
    public async Task ShouldExposeErrorAsUnionValueWhenError()
    {
        Result<int> result = TestError;
        await Assert.That(result.Value).IsEqualTo(TestError);
    }

    [Test]
    public async Task ShouldExposeUninitializedErrorAsUnionValueWhenDefault()
    {
        Result<int> result = default;
        await Assert.That(result.Value).IsEqualTo(Result.UninitializedError);
    }

    [Test]
    public async Task ShouldMatchOk()
    {
        Result<int> result = 2;
        var matched = result.Match(value => value * 10, _ => -1);
        await Assert.That(matched).IsEqualTo(20);
    }

    [Test]
    public async Task ShouldMatchError()
    {
        Result<int> result = TestError;
        var matched = result.Match(_ => "success", error => error.Code);
        await Assert.That(matched).IsEqualTo(TestError.Code);
    }

    [Test]
    public async Task ShouldMatchUninitializedErrorWhenDefault()
    {
        Result<int> result = default;
        var matched = result.Match(_ => "success", error => error.Code);
        await Assert.That(matched).IsEqualTo(Result.UninitializedError.Code);
    }

    [Test]
    public async Task ShouldNotGetErrorWhenOk()
    {
        Result<int> result = 1;
        var hasError = result.TryGetValue(out Error? error);
        await Assert.That(hasError).IsFalse();
        await Assert.That(error).IsNull();
    }

    [Test]
    public async Task ShouldNotGetValueWhenError()
    {
        Result<int> result = TestError;
        var hasValue = result.TryGetValue(out int _);
        await Assert.That(hasValue).IsFalse();
    }

    [Test]
    public async Task ShouldReturnOkFromTry()
    {
        await Result.Try(() => 1).AssertOk(1);
    }

    [Test]
    public async Task ShouldReturnUnexpectedErrorWhenTryThrows()
    {
        var exception = new InvalidOperationException("Boom");
        var result = Result.Try<int>(() => throw exception);
        await result.AssertError(new UnexpectedError("Result.Exception", "Boom", exception));
    }

    [Test]
    public async Task ShouldRethrowCancellationFromTry()
    {
        await Assert
            .That(() => Result.Try<int>(() => throw new OperationCanceledException()))
            .Throws<OperationCanceledException>();
    }

    [Test]
    public async Task ShouldReturnUnitOkFromTryAction()
    {
        var ran = false;
        var result = Result.Try(() =>
        {
            ran = true;
        });

        await result.AssertOk(Unit.Default);
        await Assert.That(ran).IsTrue();
    }

    [Test]
    public async Task ShouldReturnUnexpectedErrorWhenTryActionThrows()
    {
        var result = Result.Try(() => throw new InvalidOperationException("Boom"));
        await result.AssertUnexpected();
    }

    [Test]
    public async Task ShouldCombineAllValuesWhenAllOk()
    {
        var combined = Result.Combine(Result.Ok(1), Result.Ok(2), Result.Ok(3));

        await combined.AssertOk();
        combined.TryGetValue(out IReadOnlyList<int>? values);
        await Assert.That(values!).IsEquivalentTo([1, 2, 3]);
    }

    [Test]
    public async Task ShouldCombineToFirstError()
    {
        var combined = Result.Combine(
            Result.Ok(1),
            Result.Error<int>(TestError),
            Result.Error<int>(OtherError)
        );
        await combined.AssertError(TestError);
    }

    [Test]
    public async Task ShouldCombineEmptyToEmptyOk()
    {
        var combined = Result.Combine(Array.Empty<Result<int>>());

        combined.TryGetValue(out IReadOnlyList<int>? values);
        await Assert.That(values!).IsEmpty();
    }

    [Test]
    public async Task ShouldCombineWithMergedErrorsWhenAllOk()
    {
        var combined = Result.Combine(
            [Result.Ok(1), Result.Ok(2), Result.Ok(3)],
            errors => throw new InvalidOperationException("mergeErrors must not run on success")
        );

        await combined.AssertOk();
        combined.TryGetValue(out IReadOnlyList<int>? values);
        await Assert.That(values!).IsEquivalentTo([1, 2, 3]);
    }

    [Test]
    public async Task ShouldCombineWithMergedErrorsWhenOneIsError()
    {
        var combined = Result.Combine(
            [Result.Ok(1), Result.Error<int>(TestError), Result.Ok(3)],
            errors =>
                errors is [var only]
                    ? only
                    : throw new InvalidOperationException("expected one error")
        );

        await combined.AssertError(TestError);
    }

    [Test]
    public async Task ShouldCombineWithMergedErrorsWhenSeveralAreError()
    {
        List<Error>? seen = null;
        var merged = new Error("Merged.Error", "Multiple failures");

        var combined = Result.Combine(
            [Result.Error<int>(TestError), Result.Ok(2), Result.Error<int>(OtherError)],
            errors =>
            {
                seen = [.. errors];
                return merged;
            }
        );

        await combined.AssertError(merged);
        await Assert.That(seen!).IsEquivalentTo([TestError, OtherError]);
    }

    [Test]
    public async Task ShouldCombineWithMergedErrorsForEmptyToEmptyOk()
    {
        var combined = Result.Combine(
            Array.Empty<Result<int>>(),
            errors => throw new InvalidOperationException("mergeErrors must not run on success")
        );

        combined.TryGetValue(out IReadOnlyList<int>? values);
        await Assert.That(values!).IsEmpty();
    }

    [Test]
    public async Task ShouldCombineWithValidationErrorMergeAsMethodGroup()
    {
        var first = new ValidationError(
            "Validation.Failed",
            "One or more fields are invalid",
            new Dictionary<string, string[]> { ["Name"] = ["Required"] }
        );
        var second = new ValidationError(
            "Validation.Failed",
            "One or more fields are invalid",
            new Dictionary<string, string[]> { ["Email"] = ["Required"] }
        );

        var combined = Result.Combine(
            [Result.Ok(1), Result.Error<int>(first), Result.Error<int>(second)],
            ValidationError.Merge
        );

        await combined.AssertError(
            new ValidationError(
                "Validation.Failed",
                "One or more fields are invalid",
                new Dictionary<string, string[]>
                {
                    ["Name"] = ["Required"],
                    ["Email"] = ["Required"],
                }
            )
        );
    }

    [Test]
    public async Task ShouldCombineToNonValidationErrorWhenMergingWithValidationErrorMerge()
    {
        var validationError = new ValidationError(
            "Validation.Failed",
            "One or more fields are invalid",
            new Dictionary<string, string[]> { ["Name"] = ["Required"] }
        );
        var notFound = new NotFoundError("User.NotFound", "No user with that id");

        var combined = Result.Combine(
            [Result.Ok(1), Result.Error<int>(validationError), Result.Error<int>(notFound)],
            ValidationError.Merge
        );

        await combined.AssertError(notFound);
    }

    [Test]
    public async Task ShouldThrowWhenCombineGivenNullResults()
    {
        await Assert
            .That(() => Result.Combine((IEnumerable<Result<int>>)null!))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ShouldThrowWhenCombineWithMergedErrorsGivenNullResults()
    {
        await Assert
            .That(() => Result.Combine<int>(null!, errors => errors[0]))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ShouldThrowWhenCombineWithMergedErrorsGivenNullMergeErrors()
    {
        await Assert
            .That(() => Result.Combine(Array.Empty<Result<int>>(), null!))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ShouldReturnOkFromTryAsync()
    {
        var result = Result.TryAsync(async () =>
        {
            await Task.Yield();
            return 1;
        });

        await result.AssertOk(1);
    }

    [Test]
    public async Task ShouldReturnUnexpectedErrorWhenTryAsyncThrowsAfterAwaiting()
    {
        var exception = new InvalidOperationException("Boom");

        var result = Result.TryAsync<int>(async () =>
        {
            await Task.Yield();
            throw exception;
        });

        await result.AssertError(new UnexpectedError("Result.Exception", "Boom", exception));
    }

    [Test]
    public async Task ShouldRethrowCancellationFromTryAsync()
    {
        await Assert
            .That(() =>
                Result.TryAsync<int>(async () =>
                {
                    await Task.Yield();
                    throw new OperationCanceledException();
                })
            )
            .Throws<OperationCanceledException>();
    }

    [Test]
    public async Task ShouldReturnUnitOkFromTryAsyncWithoutValue()
    {
        var result = Result.TryAsync(async () => await Task.Yield());
        await result.AssertOk(Unit.Default);
    }

    [Test]
    public async Task ShouldReturnUnexpectedErrorWhenTryAsyncWithoutValueThrows()
    {
        var result = Result.TryAsync(async () =>
        {
            await Task.Yield();
            throw new InvalidOperationException("Boom");
        });

        await result.AssertUnexpected();
    }

    [Test]
    public async Task ShouldEqualOkWithSameValue()
    {
        Result<int> first = 1;
        Result<int> second = 1;

        await Assert.That(first == second).IsTrue();
        await Assert.That(first.Equals((object)second)).IsTrue();
        await Assert.That(first.GetHashCode()).IsEqualTo(second.GetHashCode());
    }

    [Test]
    public async Task ShouldNotEqualOkWithDifferentValue()
    {
        Result<int> first = 1;
        Result<int> second = 2;
        await Assert.That(first != second).IsTrue();
    }

    [Test]
    public async Task ShouldEqualErrorWithEqualError()
    {
        Result<int> first = new Error("Test.Error", "Something went wrong");
        Result<int> second = new Error("Test.Error", "Something went wrong");

        await Assert.That(first == second).IsTrue();
        await Assert.That(first.GetHashCode()).IsEqualTo(second.GetHashCode());
    }

    [Test]
    public async Task ShouldNotEqualErrorWithDifferentError()
    {
        Result<int> first = TestError;
        Result<int> second = OtherError;
        await Assert.That(first != second).IsTrue();
    }

    [Test]
    public async Task ShouldNotEqualOkAndError()
    {
        Result<int> success = 1;
        Result<int> failure = TestError;
        await Assert.That(success != failure).IsTrue();
    }

    [Test]
    public async Task ShouldEqualDefaultAndExplicitUninitializedError()
    {
        Result<int> fromDefault = default;
        var explicitError = Result.Error<int>(Result.UninitializedError);

        await Assert.That(fromDefault == explicitError).IsTrue();
        await Assert.That(fromDefault.GetHashCode()).IsEqualTo(explicitError.GetHashCode());
    }

    [Test]
    public async Task ShouldFormatOkAsString()
    {
        Result<int> result = 42;
        await Assert.That(result.ToString()).IsEqualTo("Ok(42)");
    }

    [Test]
    public async Task ShouldFormatErrorAsString()
    {
        Result<int> result = TestError;
        await Assert.That(result.ToString()).IsEqualTo("Error(Test.Error)");
    }

    [Test]
    public async Task ShouldFormatDefaultAsUninitializedError()
    {
        Result<int> result = default;
        await Assert.That(result.ToString()).IsEqualTo("Error(Result.Uninitialized)");
    }
}
