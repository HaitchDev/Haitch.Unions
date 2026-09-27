namespace Haitch.Unions;

/// <summary>
/// The operation conflicts with the current state.
/// </summary>
/// <param name="Code">A stable, machine-readable identifier.</param>
/// <param name="Description">A human-readable explanation.</param>
/// <exception cref="ArgumentNullException"><paramref name="Code"/> or <paramref name="Description"/> is null.</exception>
public record ConflictError(string Code, string Description) : Error(Code, Description);
