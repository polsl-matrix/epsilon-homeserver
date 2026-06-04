using Tesseract.Domain.Support.Abstractions;

namespace Tesseract.Domain.Support;

public class Contact: IContact
{
    public string? EmailAddress { get; }
    public string? MatrixId { get; }
    public string Role { get; }

    public Contact(string? emailAddress, string? matrixId, string role)
    {
        if (string.IsNullOrWhiteSpace(emailAddress) && string.IsNullOrWhiteSpace(matrixId))
        {
            throw new ArgumentException("At least one of EmailAddress or MatrixId is required.");
        }
        Role = role;
        EmailAddress = emailAddress;
        MatrixId = matrixId;
    }
}