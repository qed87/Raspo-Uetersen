namespace Raspo_Stempelkarten_Backend.Dtos;

/// <summary>
/// Represents a Team overview item for data transfer.
/// </summary>
public record TeamReadDto(string Id, string Club, string Name, List<string> Coaches);