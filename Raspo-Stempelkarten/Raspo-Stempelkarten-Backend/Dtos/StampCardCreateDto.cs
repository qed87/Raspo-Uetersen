using Microsoft.AspNetCore.Mvc;

namespace Raspo_Stempelkarten_Backend.Dtos;

public class StampCardCreateDto
{
    public string Team { get; set; } = null!;

    public string Season { get; set; } = null!;
    
    public required string Recipient { get; set; } = null!;
    
    public string[] Owners { get; set; } = [];

    public int MinStamps { get; set; } = 12;

    public int MaxStamps { get; set; } = 12;
}