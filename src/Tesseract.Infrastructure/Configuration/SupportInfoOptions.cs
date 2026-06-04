namespace Tesseract.Infrastructure.Configuration;

public class SupportInfoOptions
{
    public const string SectionName = "SupportInfo";
    public string? SupportPage { get; init; }
    public List<ContactOption> Contacts { get; set; } = new();
}

public class ContactOption
{
    public string? EmailAddress { get; set; }
    public string? MatrixId { get; set; }
    public string Role { get; set; } = string.Empty;
};