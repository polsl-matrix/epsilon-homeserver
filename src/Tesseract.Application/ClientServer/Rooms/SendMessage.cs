using MediatR;
using Tesseract.Application.ClientServer.Auth.Exceptions;
using Tesseract.Application.ClientServer.Identity.Abstractions;
using Tesseract.Application.ClientServer.Rooms.Abstractions;
using Tesseract.Application.ClientServer.Rooms.Exceptions;
using Tesseract.Application.Common.Transactions;
using Tesseract.Domain.Events;
using Tesseract.Domain.Rooms;
using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Rooms;

public static class SendMessage
{
    public sealed record Command(string RoomHandle, Guid UserId, string Body) : IRequest<Response>, ITransactional;

    internal sealed class Handler(
        IPublisher publisher,
        IEventRepository eventRepository,
        IRoomRepository roomRepository,
        IRoomMembershipRepository roomMembershipRepository,
        IRoomMessageRepository roomMessageRepository,
        IUserRepository userRepository)
        : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var senderId = new UserId(request.UserId);

            if (await GetRoomByHandleAsync(request.RoomHandle, cancellationToken) is not { } room)
            {
                throw new RoomNotFoundException(request.RoomHandle);
            }

            if (await userRepository.GetByIdAsync(senderId, cancellationToken) is not { } sender)
            {
                throw new ForbiddenException();
            }

            if (await roomMembershipRepository.GetByRoomIdAndUserIdAsync(room.Id, sender.Id, cancellationToken) is null)
            {
                throw new UserNotInRoomException();
            }

            var roomMessage = RoomMessage.Create(room, sender, request.Body);

            await roomMessageRepository.InsertAsync(roomMessage, cancellationToken);

            await PublishEventsAsync(roomMessage.Events, cancellationToken);
            var sendMessageEvent = roomMessage.Events.Single();

            roomMessage.ClearEvents();

            return new Response(sendMessageEvent.Handle);
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

    public sealed record Response(EventHandle EventHandle);
}