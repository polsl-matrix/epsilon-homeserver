using MediatR;
using Tesseract.Application.ClientServer.Auth.Exceptions;
using Tesseract.Application.ClientServer.Identity.Abstractions;
using Tesseract.Application.ClientServer.Rooms.Abstractions;
using Tesseract.Application.ClientServer.Rooms.Exceptions;
using Tesseract.Domain.Rooms;
using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Rooms;

public static class GetMessages
{
    public sealed record Query(string RoomHandle, Guid UserId) : IRequest<Response>;

    internal sealed class Handler(
        IEventRepository eventRepository,
        IRoomRepository roomRepository,
        IRoomMembershipRepository roomMembershipRepository,
        IUserRepository userRepository)
        : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
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

            var events = await eventRepository.GetByRoomIdAsync(room.Id, cancellationToken);

            return new Response(events.ToArray());
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
    }

    public sealed record Response(IReadOnlyList<string> Events);
}