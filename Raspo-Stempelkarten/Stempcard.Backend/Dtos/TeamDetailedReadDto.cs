namespace Raspo_Stempelkarten_Backend.Dtos;

/// <summary>
/// Represents a detailed team data transfer object.
/// </summary>
public record TeamDetailedReadDto(
    string Id,
    string Club,
    string Name,
    string Issuer,
    DateTimeOffset IssuedOn,
    List<string> Coaches,
    ulong ConcurrencyToken);