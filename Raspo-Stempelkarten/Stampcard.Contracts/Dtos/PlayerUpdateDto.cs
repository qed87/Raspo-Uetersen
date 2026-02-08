namespace Stampcard.Contracts.Dtos;

public record PlayerUpdateDto(Guid Id, string FirstName, string LastName, DateOnly Birthdate, 
    string Birthplace, bool Active, ulong ConcurrencyToken);
