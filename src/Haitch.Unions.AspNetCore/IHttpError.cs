namespace Haitch.Unions.AspNetCore;

/// <summary>
/// An <see cref="Error"/> that chooses its own HTTP status code.
/// </summary>
public interface IHttpError
{
    /// <summary>
    /// Gets the HTTP status code for this error.
    /// </summary>
    int StatusCode { get; }
}
