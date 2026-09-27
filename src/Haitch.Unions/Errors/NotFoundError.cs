namespace Haitch.Unions;

/// <summary>
/// The requested resource does not exist.
/// </summary>
/// <param name="Code">A stable, machine-readable identifier.</param>
/// <param name="Description">A human-readable explanation.</param>
/// <exception cref="ArgumentNullException"><paramref name="Code"/> or <paramref name="Description"/> is null.</exception>
public record NotFoundError(string Code, string Description) : Error(Code, Description);
