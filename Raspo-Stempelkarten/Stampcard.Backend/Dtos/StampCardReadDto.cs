namespace StampCard.Backend.Dtos;

/// <summary>
/// Read representation of a stamp card.
/// </summary>
/// <param name="Id"></param>
/// <param name="AccountingYear"></param>
/// <param name="Issuer"></param>
/// <param name="IssuedOn"></param>
public record StampCardReadDto(Guid Id, short AccountingYear, string Issuer, DateTimeOffset IssuedOn);