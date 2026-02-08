namespace Stampcard.Contracts.Dtos;

/// <summary>
/// A player representation for clients.
/// </summary>
public record PlayerReadDto(Guid Id, string FirstName, string LastName, DateOnly Birthdate, string Birthplace, bool Active, ulong ConcurrencyToken);