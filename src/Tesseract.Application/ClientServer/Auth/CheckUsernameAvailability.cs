using MediatR;
using Tesseract.Application.ClientServer.Auth.Exceptions;
using Tesseract.Application.ClientServer.Identity.Abstractions;
using Tesseract.Application.Common.Configuration;
using Tesseract.Domain.Common.Values;
using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Auth;

public static class CheckUsernameAvailability
{
    public sealed record Query(string? Username) : IRequest<Response>;

    public sealed record Response(bool Available);

    internal sealed class Handler(
        IMatrixConfigurationRepository configurationRepository,
        IUserRepository userRepository)
        : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            if (!Localpart.TryParse(request.Username, out var localpart))
            {
                throw new InvalidUsernameException(request.Username);
            }

            var domain = await configurationRepository.GetDomainAsync(cancellationToken);
            var handle = new UserHandle(localpart.Value, domain.Value);

            if (await userRepository.GetByHandleAsync(handle, cancellationToken) is not null)
            {
                throw new UsernameTakenException(localpart.Value);
            }

            return new Response(true);
        }
    }
}