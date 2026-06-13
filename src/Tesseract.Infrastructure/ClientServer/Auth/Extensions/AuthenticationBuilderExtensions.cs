using Microsoft.AspNetCore.Authentication;

namespace Tesseract.Infrastructure.ClientServer.Auth.Extensions;

public static class AuthenticationBuilderExtensions
{
    extension(AuthenticationBuilder builder)
    {
        public void AddOpaqueToken(string? authenticationScheme = null)
        {
            builder.AddScheme<AuthenticationSchemeOptions, OpaqueTokenAuthenticationHandler>(
                authenticationScheme, _ => { });
        }
    }
}