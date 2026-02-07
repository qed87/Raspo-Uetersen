namespace Stampcard.Contracts.Dtos;

/// <summary>
/// Represents a data transfer object for creating a new player.
/// </summary>
public record PlayerCreateDto(string FirstName, string LastName, DateOnly Birthdate, string Birthplace);