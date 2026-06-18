using System.Diagnostics.CodeAnalysis;
using Tesseract.Domain.Common.Exceptions;
using Tesseract.Domain.Common.Values;

namespace Tesseract.Domain.Users;

public record UserHandle
{
    public UserHandle(string localpart, string domain)
    {
        Localpart = localpart;
        Domain = domain;
    }

    public Localpart Localpart { get; }
    public Common.Values.Domain Domain { get; }

    public override string ToString() => $"@{Localpart}:{Domain}";

    public static bool TryParse(string? value, [NotNullWhen(true)] out UserHandle? handle)
    {
        handle = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = value.Trim();

        if (!normalized.StartsWith('@'))
        {
            return false;
        }

        var separatorIndex = normalized.IndexOf(':');

        if (separatorIndex < 1)
        {
            return false;
        }

        var localpart = normalized[1..separatorIndex].Trim();
        var domain = normalized[(separatorIndex + 1)..].Trim();

        try
        {
            handle = new UserHandle(localpart, domain);
            return true;
        }
        catch (ValidationException)
        {
            return false;
        }
    }
}