namespace Stampcard.UI.Dtos;

public record TeamDetailedReadDto(string Id, string Club, string Name,string IssuedBy, 
    string IssuedDate, List<string> Coaches, ulong ConcurrencyToken);