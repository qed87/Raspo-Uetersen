namespace Raspo_Stempelkarten_Backend.Dtos;

/// <summary>
/// A Team member representation for clients.
/// </summary>
public record MemberReadDto(Guid Id, string FirstName, string LastName, DateOnly Birthdate, string Birthplace);