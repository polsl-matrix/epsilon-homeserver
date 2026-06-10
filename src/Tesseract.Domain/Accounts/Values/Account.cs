namespace Tesseract.Domain.Accounts.Values;

public sealed record Account(string Localpart, string? PasswordHash);