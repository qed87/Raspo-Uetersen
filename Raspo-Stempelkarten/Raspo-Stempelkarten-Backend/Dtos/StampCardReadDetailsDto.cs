using JetBrains.Annotations;

namespace Raspo_Stempelkarten_Backend.Dtos;

[UsedImplicitly]
public class StampCardReadDetailsDto
{
    public Guid Id { get; set; }
    
    public required string Recipient { get; set; } = null!;

    public string Team { get; set; } = null!;

    public string Season { get; set; } = null!;

    public string IssuedBy { get; set; } = null!;
    
    public string[] Owners { get; set; } = [];

    public int MinStamps { get; set; } = 12;

    public int MaxStamps { get; set; } = 12;
    
    public int ActualStamps { get; set; }

    public List<StampReadDetailsDto> Stamps { get; set; } = null!;
}