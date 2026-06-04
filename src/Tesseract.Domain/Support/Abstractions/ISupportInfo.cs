namespace Tesseract.Domain.Support.Abstractions;

public interface ISupportInfo
{
    IReadOnlyList<IContact> Contacts { get; }
    string SupportPage { get; }
}