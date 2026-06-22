using System.Text.RegularExpressions;
using Tesseract.Domain.Common.Constants;
using Tesseract.Domain.Common.Exceptions;

namespace Tesseract.Domain.Common.Values;

public partial record Domain
{
    public Domain(string value)
    {
        var normalized = value.Trim();

        if (!IsValidDomain(normalized))
        {
            throw new ValidationException("Invalid domain format.");
        }

        Value = normalized;
    }

    public string Value { get; }

    [GeneratedRegex(Patterns.Domain)]
    private static partial Regex DomainRegex { get; }

    private static bool IsValidDomain(string value) =>
        DomainRegex.IsMatch(value);

    public override string ToString() => Value;

    public static implicit operator Domain(string value)
    {
        return new Domain(value);
    }
}