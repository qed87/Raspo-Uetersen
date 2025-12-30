using System.ComponentModel.DataAnnotations;

namespace Raspo.StampCard.Web.Models;

public class StampCard
{
    public string? Id { get; set; }
    
    [Required]
    [RegularExpression(@"\d{4}/\d{2}")]
    public string Season { get; set; }
    
    [Required]
    public string Team { get; set; }
    
    [Required]
    public string Recipient { get; set; }

    [Required]
    public int MinStamps { get; set; } = 12;
    
    [Required]
    public int MaxStamps { get; set; } = 12;

    public HashSet<string> Owners { get; set;  } = [];
}
