using MediatR;
using Tesseract.Domain.Users.Entities;
using Tesseract.Domain.Users.Values;

namespace Tesseract.Application.ClientServer.Auth;

public static class LoginUser
{
    public record Command(string? User, string? Password, string Type) : IRequest<Response>;

    internal sealed class Handler : IRequestHandler<Command, Response>
    {
        public Task<Response> Handle(Command request, CancellationToken cancellationToken) =>
            Task.FromResult(new Response(new User(Guid.Empty, new Handle("dummy", "dummy"))));
    }

    public record Response(User User);
}