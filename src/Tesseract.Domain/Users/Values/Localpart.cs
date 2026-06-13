using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Tesseract.Domain.Common.Exceptions;

namespace Tesseract.Domain.Users.Values;

public partial record Localpart
{
    public Localpart(string value)
    {
        var normalized = value.Trim();

        if (!IsValidLocalpart(normalized))
        {
            throw new ValidationException("Invalid localpart format.");
        }

        Value = normalized;
    }

    public string Value { get; }

    [GeneratedRegex(@"^[0-9a-z\-.=_/+]{1,255}$")]
    private static partial Regex LocalpartRegex { get; }

    private static bool IsValidLocalpart(string value)
        => LocalpartRegex.IsMatch(value);

    public override string ToString() => Value;

    public static implicit operator Localpart(string value)
    {
        return new Localpart(value);
    }

    public static bool TryParse(string? value, [NotNullWhen(true)] out Localpart? localpart)
    {
        localpart = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        try
        {
            localpart = new Localpart(value);
            return true;
        }
        catch (ValidationException)
        {
            return false;
        }
    }
}