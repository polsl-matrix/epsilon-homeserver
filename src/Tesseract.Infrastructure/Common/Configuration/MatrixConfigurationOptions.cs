using System.ComponentModel.DataAnnotations;
using Tesseract.Domain.Common.Constants;

namespace Tesseract.Infrastructure.Common.Configuration;

public class MatrixConfigurationOptions
{
    public const string SectionName = "Matrix";

    [Required]
    [RegularExpression(Patterns.Domain)]
    public required string Domain { get; init; }
}