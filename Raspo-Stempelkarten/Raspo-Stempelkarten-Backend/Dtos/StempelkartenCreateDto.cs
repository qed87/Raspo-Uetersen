using Microsoft.AspNetCore.Mvc;

namespace Raspo_Stempelkarten_Backend.Dtos;

public class StempelkartenCreateDto
{
    public required string Recipient { get; set; } = null!;

    [FromRoute(Name = "Team")] public string Team { get; set; } = null!;

    [FromRoute(Name = "Season")] public string Season { get; set; } = null!;
    
    public string[] AdditionalOwner { get; set; } = [];

    public int MinStamps { get; set; } = 12;

    public int MaxStamps { get; set; } = 12;
}