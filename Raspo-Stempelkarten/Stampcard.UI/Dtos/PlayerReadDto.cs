namespace Stampcard.UI.Dtos;

/// <summary>
/// A player representation for clients.
/// </summary>
public record PlayerReadDto(Guid Id, string FirstName, string LastName, DateOnly Birthdate, string Birthplace);