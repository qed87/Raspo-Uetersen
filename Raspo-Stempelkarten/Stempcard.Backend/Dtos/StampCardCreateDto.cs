namespace Raspo_Stempelkarten_Backend.Dtos;

/// <summary>
/// Transfer object for creating a new stamp card.
/// </summary>
/// <param name="MemberId"></param>
/// <param name="AccountingYear"></param>
public record StampCardCreateDto(Guid MemberId, short AccountingYear);