namespace Haitch.Unions;

/// <summary>
/// The input was invalid.
/// </summary>
/// <param name="Code">A stable, machine-readable identifier.</param>
/// <param name="Description">A human-readable explanation.</param>
/// <param name="Failures">The failure messages for each invalid field; null means none.</param>
/// <exception cref="ArgumentNullException"><paramref name="Code"/> or <paramref name="Description"/> is null.</exception>
public record ValidationError(
    string Code,
    string Description,
    IReadOnlyDictionary<string, string[]>? Failures = null
) : Error(Code, Description)
{
    private const string DefaultCode = "Validation.Failed";
    private const string DefaultDescription = "One or more fields are invalid";
    private const string NullElementMessage =
        "The errors collection must not contain null elements.";

    /// <summary>
    /// The failure messages for each invalid field.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Failures
    {
        get;
        init => field = value ?? new Dictionary<string, string[]>();
    } = Failures ?? new Dictionary<string, string[]>();

    /// <inheritdoc />
    public virtual bool Equals(ValidationError? other) =>
        other is not null
        && base.Equals(other)
        && Failures.Count == other.Failures.Count
        && Failures.All(pair =>
            other.Failures.TryGetValue(pair.Key, out var messages)
            && pair.Value.SequenceEqual(messages)
        );

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Failures.Count);

    /// <summary>
    /// Merges <paramref name="errors"/> into one, joining failures by field, with the default code and description.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="errors"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="errors"/> contains a null element.</exception>
    public static ValidationError Merge(params IReadOnlyList<ValidationError> errors) =>
        Merge(DefaultCode, DefaultDescription, errors);

    /// <summary>
    /// Merges <paramref name="errors"/> into one with the given code and description, joining failures by field.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="errors"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="errors"/> contains a null element.</exception>
    public static ValidationError Merge(
        string code,
        string description,
        params IReadOnlyList<ValidationError> errors
    )
    {
        ArgumentNullException.ThrowIfNull(errors);

        var failures = new Dictionary<string, string[]>();
        foreach (var error in errors)
        {
            if (error is null)
            {
                throw new ArgumentException(NullElementMessage, nameof(errors));
            }

            foreach (var (field, messages) in error.Failures)
            {
                failures[field] = failures.TryGetValue(field, out var existing)
                    ? [.. existing, .. messages]
                    : [.. messages];
            }
        }

        return new ValidationError(code, description, failures);
    }

    /// <summary>
    /// Merges <paramref name="errors"/> for <c>Result.Combine</c>: the first non-validation error if any,
    /// otherwise every validation error merged with the default code and description.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="errors"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="errors"/> contains a null element.</exception>
    public static Error Merge(IReadOnlyList<Error> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        var validationErrors = new ValidationError[errors.Count];
        for (var i = 0; i < errors.Count; i++)
        {
            var error = errors[i];
            if (error is null)
            {
                throw new ArgumentException(NullElementMessage, nameof(errors));
            }

            if (error is not ValidationError validationError)
            {
                return error;
            }

            validationErrors[i] = validationError;
        }

        return Merge(DefaultCode, DefaultDescription, validationErrors);
    }
}
