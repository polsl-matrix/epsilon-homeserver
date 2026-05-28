using Tesseract.Domain.Common.Exceptions;

namespace Tesseract.Domain.Users.Values;

public sealed record Handle
{
    public Handle(string localpart, string domain)
    {
        Localpart = localpart;
        Domain = domain;
    }

    public Localpart Localpart { get; }
    public Domain Domain { get; }

    public override string ToString() => $"@{Localpart}:{Domain}";

    public static Handle Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException("Handle cannot be empty.");
        }

        var normalized = value.TrimStart();

        if (!normalized.StartsWith('@'))
        {
            throw new ValidationException("Invalid handle format.");
        }

        var separatorIndex = normalized.IndexOf(':');

        if (separatorIndex < 1)
        {
            throw new ValidationException("Invalid handle format.");
        }

        var localpart = normalized[1..separatorIndex].Trim();
        var domain = normalized[(separatorIndex + 1)..].Trim();

        return new Handle(localpart, domain);
    }
}