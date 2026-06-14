namespace Tesseract.Application.ClientServer.Discovery.Models;

public sealed record DiscoveryInfo(Uri HomeserverBaseUrl, Uri? IdentityServerBaseUrl);