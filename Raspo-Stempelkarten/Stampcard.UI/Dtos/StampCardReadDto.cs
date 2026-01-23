using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Raspo.StampCard.Web.Dtos;

[UsedImplicitly]
public record StampCardReadDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("recipient")]
    public string Recipient { get; set; } = null!;

    [JsonPropertyName("team")]
    public string Team { get; set; } = null!;

    [JsonPropertyName("season")]
    public string Season { get; set; } = null!;

    [JsonPropertyName("issuedBy")]
    public string IssuedBy { get; set; } = null!;

    [JsonPropertyName("minStamps")]
    public int MinStamps { get; set; } = 12;

    [JsonPropertyName("maxStamps")]
    public int MaxStamps { get; set; } = 12;

    public string[] Owners { get; set; } = [];
}