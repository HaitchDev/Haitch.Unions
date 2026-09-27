namespace Haitch.Unions;

/// <summary>
/// The caller is not allowed to perform the operation.
/// </summary>
/// <param name="Code">A stable, machine-readable identifier.</param>
/// <param name="Description">A human-readable explanation.</param>
/// <exception cref="ArgumentNullException"><paramref name="Code"/> or <paramref name="Description"/> is null.</exception>
public record ForbiddenError(string Code, string Description) : Error(Code, Description);
