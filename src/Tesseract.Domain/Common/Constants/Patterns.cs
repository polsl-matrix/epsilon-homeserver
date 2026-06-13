namespace Tesseract.Domain.Common.Constants;

public static class Patterns
{
    // @formatter:off
    public const string Domain = @"^(?:(?:\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})|(?:\[[0-9A-Fa-f:.]{2,45}\])|(?:[0-9A-Za-z-.]{1,255}))(?::\d{1,5})?$";
    public const string Localpart = @"^[0-9a-z\-.=_/+]{1,255}$";
    // @formatter:on
}