namespace Tesseract.Infrastructure.ClientServer.Common.Dao;

internal sealed class ResponseDao
{
    public required Guid UserId { get; init; }
    public required int StatusCode { get; init; }
    public required string? ContentType { get; init; }
    public required string Content { get; init; }
}