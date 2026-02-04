namespace StampCard.Backend.Dtos;

/// <summary>
/// Represents a stamp record for data transfer.
/// </summary>
/// <param name="Id"></param>
/// <param name="Reason"></param>
public record StampReadDto(Guid Id, string Reason, string Issuer, DateTimeOffset IssuedOn);