namespace Tesseract.Domain.Support.Abstractions;

public interface IContact
{
    string? EmailAddress { get; }
    string? MatrixId { get; }
    string Role { get; }
}