using MediatR;
using Tesseract.Domain.Rooms;
using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Rooms;

public static class GetJoinedRooms
{
    public sealed record Query(UserId UserId) : IRequest<Response>;

    internal sealed class Handler(
        IRoomMembershipRepository roomMembershipRepository)
        : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var rooms = await roomMembershipRepository
                .GetRoomsByUserIdAsync(request.UserId, cancellationToken);

            var handles = rooms.Select(r => r.Handle);

            return new Response([.. handles]);
        }
    }

    public sealed record Response(IReadOnlyList<RoomHandle> RoomHandles);
}