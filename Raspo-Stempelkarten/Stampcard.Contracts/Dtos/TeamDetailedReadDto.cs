namespace Stampcard.Contracts.Dtos;

/// <summary>
/// Represents a detailed team data transfer object.
/// </summary>
public record TeamDetailedReadDto(
    string Id,
    string Club,
    string Name,
    string Issuer,
    DateTimeOffset IssuedOn,
    bool Deleted,
    List<string> Coaches,
    ulong ConcurrencyToken);