using MediatR;
using Tesseract.Application.ClientServer.Rooms.Abstractions;
using Tesseract.Application.Common.Configuration;
using Tesseract.Domain.Rooms;
using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Rooms;

public static class CreateRoom
{
    public sealed record Command(UserId CreatorId) : IRequest<Response>;

    internal sealed class Handler(
        IPublisher publisher,
        IMatrixConfigurationRepository configurationRepository,
        IRoomRepository roomRepository,
        IEventRepository eventRepository)
        : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var domain = await configurationRepository.GetDomainAsync(cancellationToken);
            var room = Room.Create(request.CreatorId, domain);

            await roomRepository.SaveAsync(room, cancellationToken);

            foreach (var @event in room.Events)
            {
                await eventRepository.InsertAsync(@event, cancellationToken);
                await publisher.Publish(@event, cancellationToken);
            }

            room.ClearEvents();
            return new Response(room.Handle);
        }
    }

    public sealed record Response(RoomHandle Handle);
}