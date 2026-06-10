namespace Tesseract.Domain.Accounts.Values;

public sealed record Device(string DeviceId, string? DisplayName, string AccessToken);