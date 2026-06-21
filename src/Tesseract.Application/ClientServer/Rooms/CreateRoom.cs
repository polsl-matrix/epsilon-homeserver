using MediatR;
using Tesseract.Application.ClientServer.Auth.Exceptions;
using Tesseract.Application.ClientServer.Identity.Abstractions;
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
        IEventRepository eventRepository,
        IMatrixConfigurationRepository configurationRepository,
        IRoomRepository roomRepository,
        IUserRepository userRepository)
        : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var creator = await userRepository.GetByIdAsync(request.CreatorId, cancellationToken);
            var domain = await configurationRepository.GetDomainAsync(cancellationToken);

            if (creator is null)
            {
                throw new ForbiddenException();
            }

            var room = Room.Create(creator, domain);

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