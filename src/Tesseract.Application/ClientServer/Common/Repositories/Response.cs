using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Common.Repositories;

public sealed class Response
{
    public required UserId UserId { get; init; }
    public required string Path { get; init; }

    public required int StatusCode { get; init; }
    public required string? ContentType { get; init; }
    public required string Content { get; init; }
}