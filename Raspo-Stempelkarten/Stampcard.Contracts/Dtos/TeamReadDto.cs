namespace Stampcard.Contracts.Dtos;

/// <summary>
/// Represents a Team overview item for data transfer.
/// </summary>
public record TeamReadDto(string Id, string Club, string Name, List<string> Coaches);