using MediatR;
using Tesseract.Domain.Rooms;

namespace Tesseract.Application.ClientServer.Rooms;

public static class CreateRoom
{
    public sealed record Command : IRequest<Response>;

    internal sealed class Handler : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken) =>
            new(new RoomHandle("localpart", "domain"));
    }

    public sealed record Response(RoomHandle Handle);
}