using Tesseract.Domain.Common.Values;
using VDomain = Tesseract.Domain.Common.Values.Domain;

namespace Tesseract.Domain.Rooms;

public class RoomHandle
{
    public RoomHandle(string localpart, string domain)
    {
        Localpart = localpart;
        Domain = domain;
    }

    public Localpart Localpart { get; }
    public VDomain Domain { get; }

    public override string ToString() => $"!{Localpart}:{Domain}";
}