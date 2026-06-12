using System.Text.RegularExpressions;
using Tesseract.Domain.Common.Exceptions;

namespace Tesseract.Domain.Users.Values;

public readonly partial record struct Domain
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

    // @formatter:off
    [GeneratedRegex(@"^(?:(?:\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})|(?:\[[0-9A-Fa-f:.]{2,45}\])|(?:[0-9A-Za-z-.]{1,255}))(?::\d{1,5})?$")]
    private static partial Regex DomainRegex { get; }
    // @formatter:on

    private static bool IsValidDomain(string value) =>
        DomainRegex.IsMatch(value);

    public override string ToString() => Value;

    public static implicit operator Domain(string value)
    {
        return new Domain(value);
    }
}