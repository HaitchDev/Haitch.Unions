namespace Haitch.Unions;

/// <summary>
/// An unexpected error occurred.
/// </summary>
/// <param name="Code">A stable, machine-readable identifier.</param>
/// <param name="Description">A human-readable explanation.</param>
/// <param name="Exception">The exception that caused the error, if any.</param>
/// <exception cref="ArgumentNullException"><paramref name="Code"/> or <paramref name="Description"/> is null.</exception>
public record UnexpectedError(string Code, string Description, Exception? Exception = null)
    : Error(Code, Description);
