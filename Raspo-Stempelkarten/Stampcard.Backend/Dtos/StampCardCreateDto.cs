namespace StampCard.Backend.Dtos;

/// <summary>
/// Transfer object for creating a new stamp card.
/// </summary>
/// <param name="PlayerId"></param>
/// <param name="AccountingYear"></param>
public record StampCardCreateDto(Guid PlayerId, short AccountingYear);