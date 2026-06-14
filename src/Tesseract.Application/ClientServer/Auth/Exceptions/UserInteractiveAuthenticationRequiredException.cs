namespace Tesseract.Application.ClientServer.Auth.Exceptions;

public sealed class UserInteractiveAuthenticationRequiredException(
    IReadOnlyList<IReadOnlyList<string>> flows,
    string session,
    IReadOnlyList<string>? completed = null,
    string? errorCode = null,
    string? error = null)
    : Exception(error ?? "User-interactive authentication is required.")
{
    public IReadOnlyList<IReadOnlyList<string>> Flows { get; } = flows;
    public string Session { get; } = session;
    public IReadOnlyList<string> Completed { get; } = completed ?? [];
    public string? ErrorCode { get; } = errorCode;
    public string? Error { get; } = error;
}