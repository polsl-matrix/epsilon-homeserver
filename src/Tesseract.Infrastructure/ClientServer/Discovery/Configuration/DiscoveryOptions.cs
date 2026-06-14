using System.ComponentModel.DataAnnotations;

namespace Tesseract.Infrastructure.ClientServer.Discovery.Configuration;

public sealed class DiscoveryOptions
{
    public const string SectionName = "Discovery";

    [Required]
    public required Uri HomeserverBaseUrl { get; init; }

    public Uri? IdentityServerBaseUrl { get; init; }
}