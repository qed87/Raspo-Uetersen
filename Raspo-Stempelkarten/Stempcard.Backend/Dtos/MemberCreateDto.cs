namespace Raspo_Stempelkarten_Backend.Dtos;

/// <summary>
/// Represents a data transfer object for creating a new team member.
/// </summary>
public record MemberCreateDto(string FirstName, string LastName, DateOnly Birthdate, string Birthplace);