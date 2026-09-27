using Haitch.Unions.TUnit.Assertions;

namespace Haitch.Unions.UnitTests.Errors;

public class ErrorTests
{
    public static IEnumerable<Func<(Error Error, string Kind)>> ErrorKinds()
    {
        yield return () => (new NotFoundError("Code", "Description"), "NotFound");
        yield return () => (CreateValidationError("Name", "Required"), "Validation");
        yield return () => (new ConflictError("Code", "Description"), "Conflict");
        yield return () => (new UnauthorizedError("Code", "Description"), "Unauthorized");
        yield return () => (new ForbiddenError("Code", "Description"), "Forbidden");
        yield return () => (new UnexpectedError("Code", "Description"), "Unexpected");
        yield return () => (new Error("Code", "Description"), "General");
    }

    [Test]
    [MethodDataSource(nameof(ErrorKinds))]
    public async Task ShouldMatchErrorKind((Error Error, string Kind) testCase)
    {
        var kind = testCase.Error switch
        {
            NotFoundError => "NotFound",
            ValidationError => "Validation",
            ConflictError => "Conflict",
            UnauthorizedError => "Unauthorized",
            ForbiddenError => "Forbidden",
            UnexpectedError => "Unexpected",
            _ => "General",
        };
        await Assert.That(kind).IsEqualTo(testCase.Kind);
    }

    [Test]
    public async Task ShouldNotEqualBaseErrorWithSameCodeAndDescription()
    {
        Error notFound = new NotFoundError("Code", "Description");
        await Assert.That(notFound).IsNotEqualTo(new Error("Code", "Description"));
    }

    [Test]
    public async Task ShouldEqualValidationErrorWithSameFailures()
    {
        var first = CreateValidationError("Name", "Required");
        var second = CreateValidationError("Name", "Required");
        await Assert.That(first).IsEqualTo(second);
        await Assert.That(first.GetHashCode()).IsEqualTo(second.GetHashCode());
    }

    [Test]
    public async Task ShouldNotEqualValidationErrorWithDifferentMessages()
    {
        var first = CreateValidationError("Name", "Required");
        var second = CreateValidationError("Name", "Too long");
        await Assert.That(first).IsNotEqualTo(second);
    }

    [Test]
    public async Task ShouldNotEqualValidationErrorWithDifferentFields()
    {
        var first = CreateValidationError("Name", "Required");
        var second = CreateValidationError("Email", "Required");
        await Assert.That(first).IsNotEqualTo(second);
    }

    [Test]
    public async Task ShouldCompareAndMergeValidationErrorConstructedWithNullFailures()
    {
        var withNullFailures = new ValidationError("c", "d", null!);

        await Assert
            .That(withNullFailures)
            .IsEqualTo(new ValidationError("c", "d", new Dictionary<string, string[]>()));

        var merged = ValidationError.Merge(
            withNullFailures,
            CreateValidationError("Name", "Required")
        );
        await Assert.That(merged.Failures["Name"]).IsEquivalentTo(["Required"]);
    }

    [Test]
    public async Task ShouldDefaultFailuresToEmptyWhenOmitted()
    {
        var validationError = new ValidationError("c", "d");
        await Assert.That(validationError.Failures).IsEmpty();
    }

    [Test]
    public async Task ShouldThrowWhenCodeIsNull()
    {
        await Assert.That(() => new Error(null!, "Description")).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ShouldThrowWhenDescriptionIsNull()
    {
        await Assert.That(() => new Error("Code", null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ShouldKeepRuntimeTypeWhenUsingWithOnDescription()
    {
        Error notFound = new NotFoundError("Code", "Description");

        var updated = notFound with { Description = "Wrapped: Description" };

        await Assert.That(updated).IsTypeOf<NotFoundError>();
        await Assert.That(updated.Description).IsEqualTo("Wrapped: Description");
    }

    [Test]
    public async Task ShouldThrowWhenWithSetsCodeToNull()
    {
        var error = new Error("Code", "Description");

        await Assert.That(() => error with { Code = null! }).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ShouldDefaultFailuresToEmptyWhenWithSetsFailuresToNull()
    {
        var validationError = CreateValidationError("Name", "Required");

        var updated = validationError with { Failures = null! };

        await Assert.That(updated.Failures).IsEmpty();
    }

    [Test]
    public async Task ShouldReplaceFailuresWhenUsingWith()
    {
        var validationError = CreateValidationError("Name", "Required");

        var updated = validationError with
        {
            Failures = new Dictionary<string, string[]> { ["Email"] = ["Required"] },
        };

        await Assert.That(updated.Failures.ContainsKey("Email")).IsTrue();
        await Assert.That(updated.Failures.ContainsKey("Name")).IsFalse();
    }

    [Test]
    public async Task ShouldHoldErrorSubtypeInResult()
    {
        Result<int> result = new NotFoundError("User.NotFound", "No user with that id");
        await result.AssertError(new NotFoundError("User.NotFound", "No user with that id"));
    }

    [Test]
    public async Task ShouldHoldValidationErrorInResult()
    {
        Result<int> result = CreateValidationError("Name", "Required");
        await result.AssertError(CreateValidationError("Name", "Required"));
    }

    [Test]
    public async Task ShouldMergeValidationErrorsFromDifferentFields()
    {
        var first = CreateValidationError("Name", "Required");
        var second = CreateValidationError("Email", "Required");

        var merged = ValidationError.Merge(first, second);

        await Assert
            .That(merged)
            .IsEqualTo(
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
    public async Task ShouldJoinFailuresInOrderWhenMergingSameField()
    {
        var first = CreateValidationError("Name", "Required");
        var second = CreateValidationError("Name", "Too long");

        var merged = ValidationError.Merge(first, second);

        await Assert.That(merged.Failures["Name"]).IsEquivalentTo(["Required", "Too long"]);
    }

    [Test]
    public async Task ShouldReplaceCodeAndDescriptionWhenMergingSingleValidationErrorWithoutOverride()
    {
        var only = new ValidationError(
            "Custom.Code",
            "Custom description",
            new Dictionary<string, string[]> { ["Name"] = ["Required"] }
        );

        var merged = ValidationError.Merge(only);

        await Assert.That(merged.Code).IsEqualTo("Validation.Failed");
        await Assert.That(merged.Description).IsEqualTo("One or more fields are invalid");
        await Assert.That(merged.Failures["Name"]).IsEquivalentTo(["Required"]);
    }

    [Test]
    public async Task ShouldMergeWithCustomCodeAndDescription()
    {
        var first = CreateValidationError("Name", "Required");
        var second = CreateValidationError("Email", "Required");

        var merged = ValidationError.Merge("Custom.Code", "Custom description", first, second);

        await Assert.That(merged.Code).IsEqualTo("Custom.Code");
        await Assert.That(merged.Description).IsEqualTo("Custom description");
    }

    [Test]
    public async Task ShouldMergeNoValidationErrorsToNoFailures()
    {
        var merged = ValidationError.Merge();
        await Assert.That(merged.Failures).IsEmpty();
    }

    [Test]
    public async Task ShouldNotShareFailureArrayWithInput()
    {
        var only = CreateValidationError("Name", "Required");

        var merged = ValidationError.Merge(only);

        await Assert
            .That(ReferenceEquals(merged.Failures["Name"], only.Failures["Name"]))
            .IsFalse();
    }

    [Test]
    public async Task ShouldThrowWhenMergeGivenNullErrorsList()
    {
        await Assert
            .That(() => ValidationError.Merge((IReadOnlyList<ValidationError>)null!))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ShouldThrowWhenMergeGivenNullElement()
    {
        await Assert
            .That(() => ValidationError.Merge(CreateValidationError("Name", "Required"), null!))
            .Throws<ArgumentException>();
    }

    [Test]
    public async Task ShouldThrowWhenMergeWithCodeAndDescriptionGivenNullElement()
    {
        await Assert
            .That(() =>
                ValidationError.Merge(
                    "Custom.Code",
                    "Custom description",
                    CreateValidationError("Name", "Required"),
                    null!
                )
            )
            .Throws<ArgumentException>();
    }

    [Test]
    public async Task ShouldMergeListOfValidationErrorsAsValidationError()
    {
        List<ValidationError> errors =
        [
            CreateValidationError("Name", "Required"),
            CreateValidationError("Email", "Required"),
        ];

        var merged = ValidationError.Merge(errors);

        // Only compiles if Merge(List<ValidationError>) resolves to the ValidationError-returning overload.
        await Assert.That(merged.Failures.Count).IsEqualTo(2);
    }

    [Test]
    public async Task ShouldMergeListOfValidationErrorsWithCustomCodeAsValidationError()
    {
        List<ValidationError> errors = [CreateValidationError("Name", "Required")];

        var merged = ValidationError.Merge("Custom.Code", "Custom description", errors);

        // Only compiles if Merge(string, string, List<ValidationError>) resolves to the ValidationError overload.
        await Assert.That(merged.Code).IsEqualTo("Custom.Code");
        await Assert.That(merged.Failures.Count).IsEqualTo(1);
    }

    [Test]
    public async Task ShouldMergeErrorListWhenEveryErrorIsAValidationError()
    {
        var first = CreateValidationError("Name", "Required");
        var second = CreateValidationError("Email", "Required");

        var merged = ValidationError.Merge((IReadOnlyList<Error>)[first, second]);

        await Assert
            .That(merged)
            .IsEqualTo(
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
    public async Task ShouldReturnFirstNonValidationErrorFromMixedErrorList()
    {
        var validationError = CreateValidationError("Name", "Required");
        var notFound = new NotFoundError("User.NotFound", "No user with that id");
        var other = new Error("Other.Error", "Something else went wrong");

        var merged = ValidationError.Merge(
            (IReadOnlyList<Error>)[validationError, notFound, other]
        );

        await Assert.That(merged).IsEqualTo(notFound);
    }

    [Test]
    public async Task ShouldReturnFirstNonValidationErrorWhenItIsFirstInList()
    {
        var notFound = new NotFoundError("User.NotFound", "No user with that id");
        var validationError = CreateValidationError("Name", "Required");

        var merged = ValidationError.Merge((IReadOnlyList<Error>)[notFound, validationError]);

        await Assert.That(merged).IsEqualTo(notFound);
    }

    [Test]
    public async Task ShouldMergeEmptyErrorListToValidationErrorWithNoFailures()
    {
        var merged = ValidationError.Merge((IReadOnlyList<Error>)[]);

        await Assert.That(merged is ValidationError { Failures.Count: 0 }).IsTrue();
    }

    [Test]
    public async Task ShouldThrowWhenMergeErrorListGivenNull()
    {
        await Assert
            .That(() => ValidationError.Merge((IReadOnlyList<Error>)null!))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ShouldThrowWhenMergeErrorListGivenNullElement()
    {
        await Assert
            .That(() =>
                ValidationError.Merge(
                    (IReadOnlyList<Error>)[CreateValidationError("Name", "Required"), null!]
                )
            )
            .Throws<ArgumentException>();
    }

    private static ValidationError CreateValidationError(string field, string message) =>
        new(
            "Validation.Failed",
            "One or more fields are invalid",
            new Dictionary<string, string[]> { [field] = [message] }
        );
}
