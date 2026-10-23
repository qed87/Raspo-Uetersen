namespace Stampcard.UI.Clients;

/// <summary>
/// A team as returned by the backend.
/// </summary>
public record TeamReadDto(string Id, string Club, string Name, List<string> Coaches);

/// <summary>
/// A player as returned by the backend.
/// </summary>
public record PlayerReadDto(Guid Id, string FirstName, string LastName, DateOnly Birthdate, bool Active);

/// <summary>
/// A stamp card including its stamps as returned by the backend.
/// </summary>
public record StampCardReadDto(Guid Id, Guid PlayerId, int AccountingYear, List<StampReadDto> Stamps);

/// <summary>
/// A stamp as returned by the backend. The status is either <c>pending</c> or <c>accepted</c>.
/// </summary>
public record StampReadDto(Guid Id, string Reason, string Issuer, DateTimeOffset IssuedOn, string Status);
