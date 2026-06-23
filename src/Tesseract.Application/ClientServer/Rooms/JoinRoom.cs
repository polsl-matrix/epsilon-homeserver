using MediatR;
using Tesseract.Application.ClientServer.Auth.Exceptions;
using Tesseract.Application.ClientServer.Identity.Abstractions;
using Tesseract.Application.ClientServer.Rooms.Abstractions;
using Tesseract.Application.ClientServer.Rooms.Exceptions;
using Tesseract.Domain.Events;
using Tesseract.Domain.Rooms;
using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Rooms;

public static class JoinRoom
{
    public sealed record Command(string RoomHandle, UserId UserId) : IRequest<Response>;

    internal sealed class Handler(
        IPublisher publisher,
        IEventRepository eventRepository,
        IRoomRepository roomRepository,
        IRoomMembershipRepository roomMembershipRepository,
        IUserRepository userRepository)
        : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            if (await GetRoomByHandleAsync(request.RoomHandle, cancellationToken) is not { } room)
            {
                throw new RoomNotFoundException(request.RoomHandle);
            }

            if (await userRepository.GetByIdAsync(request.UserId, cancellationToken) is not { } sender)
            {
                throw new ForbiddenException();
            }

            if (await roomMembershipRepository.GetByRoomIdAndUserIdAsync(room.Id, sender.Id, cancellationToken)
                is not null)
            {
                return new Response(room.Handle);
            }

            var membership = RoomMembership.Create(room, sender);

            await roomMembershipRepository.InsertAsync(membership, cancellationToken);

            await PublishEventsAsync(membership.Events, cancellationToken);

            membership.ClearEvents();

            return new Response(room.Handle);
        }

        private async Task<Room?> GetRoomByHandleAsync(string handle, CancellationToken cancellationToken)
        {
            if (!RoomHandle.TryParse(handle, out var roomHandle))
            {
                return null;
            }

            if (await roomRepository.GetByHandleAsync(roomHandle, cancellationToken) is not { } room)
            {
                return null;
            }

            return room;
        }

        private async Task PublishEventsAsync(IEnumerable<Event> events, CancellationToken cancellationToken)
        {
            foreach (var @event in events)
            {
                await eventRepository.InsertAsync(@event, cancellationToken);
                await publisher.Publish(@event, cancellationToken);
            }
        }
    }

    public sealed record Response(RoomHandle Handle);
}