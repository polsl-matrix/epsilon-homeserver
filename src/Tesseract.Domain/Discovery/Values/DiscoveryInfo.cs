namespace Tesseract.Domain.Discovery.Values;

public sealed record DiscoveryInfo(Uri HomeserverBaseUrl, Uri? IdentityServerBaseUrl);