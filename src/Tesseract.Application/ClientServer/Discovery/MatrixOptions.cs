namespace Tesseract.Application.ClientServer.Discovery;

public sealed class MatrixOptions
{
    public HomeserverOptions? Homeserver { get; set; }

    public IdentityServerOptions? IdentityServer { get; set; }
}

public sealed class HomeserverOptions
{
    public required string BaseUrl { get; set; }
}

public sealed class IdentityServerOptions
{
    public required string BaseUrl { get; set; }
}