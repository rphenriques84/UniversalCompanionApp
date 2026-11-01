using System.ComponentModel.DataAnnotations;

namespace Companion.Frontend.Features.Backend;

public sealed class BackendOptions
{
    public const string SectionName = "Backend";

    [Required]
    [Url]
    public string BaseUrl { get; set; } = string.Empty;
}
