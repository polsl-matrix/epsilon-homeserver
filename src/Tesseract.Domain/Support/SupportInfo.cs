using System.Collections;
using Tesseract.Domain.Support.Abstractions;

namespace Tesseract.Domain.Support;

public class SupportInfo(IReadOnlyList<IContact> contacts, string supportPage) : ISupportInfo
{
    public IReadOnlyList<IContact> Contacts { get; } = contacts;
    public string SupportPage { get; } = supportPage;
}