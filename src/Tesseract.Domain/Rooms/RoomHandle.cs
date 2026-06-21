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
}