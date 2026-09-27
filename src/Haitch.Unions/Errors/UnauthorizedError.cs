namespace Haitch.Unions;

/// <summary>
/// The caller is not authenticated.
/// </summary>
/// <param name="Code">A stable, machine-readable identifier.</param>
/// <param name="Description">A human-readable explanation.</param>
/// <exception cref="ArgumentNullException"><paramref name="Code"/> or <paramref name="Description"/> is null.</exception>
public record UnauthorizedError(string Code, string Description) : Error(Code, Description);
