using System.Diagnostics.CodeAnalysis;
using Tesseract.Domain.Common.Exceptions;
using Tesseract.Domain.Common.Values;
using VDomain = Tesseract.Domain.Common.Values.Domain;

namespace Tesseract.Domain.Rooms;

public class RoomHandle(string localpart, string domain)
{
    public Localpart Localpart { get; } = localpart;
    public VDomain Domain { get; } = domain;

    public override string ToString() => $"!{Localpart}:{Domain}";

    public static RoomHandle Random(string domain)
    {
        const int length = 32;

        var localpart = Localpart.Random(length);
        return new RoomHandle(localpart.Value, domain);
    }

    public static bool TryParse(string? value, [NotNullWhen(true)] out RoomHandle? handle)
    {
        handle = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = value.Trim();

        if (!normalized.StartsWith('!'))
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
            handle = new RoomHandle(localpart, domain);
            return true;
        }
        catch (ValidationException)
        {
            return false;
        }
    }
}