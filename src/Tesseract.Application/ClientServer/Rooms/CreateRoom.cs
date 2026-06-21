using MediatR;
using Tesseract.Application.ClientServer.Auth.Exceptions;
using Tesseract.Application.ClientServer.Identity.Abstractions;
using Tesseract.Application.ClientServer.Rooms.Abstractions;
using Tesseract.Application.Common.Configuration;
using Tesseract.Application.Common.Transactions;
using Tesseract.Domain.Events;
using Tesseract.Domain.Rooms;
using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Rooms;

public static class CreateRoom
{
    public sealed record Command(UserId CreatorId) : IRequest<Response>, ITransactional;

    internal sealed class Handler(
        IPublisher publisher,
        IEventRepository eventRepository,
        IMatrixConfigurationRepository configurationRepository,
        IRoomMembershipRepository roomMembershipRepository,
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
            var membership = RoomMembership.Create(room, creator);

            await roomRepository.InsertAsync(room, cancellationToken);
            await roomMembershipRepository.InsertAsync(membership, cancellationToken);

            await PublishEventsAsync(room.Events, cancellationToken);
            await PublishEventsAsync(membership.Events, cancellationToken);

            room.ClearEvents();
            membership.ClearEvents();

            return new Response(room.Handle);
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