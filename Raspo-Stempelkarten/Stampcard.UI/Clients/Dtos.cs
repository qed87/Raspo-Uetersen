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
/// A stamp card as returned to an external user, without the issuers of its stamps.
/// Stamps the external user submits are issued in the name of the holder of the share code.
/// </summary>
public record SharedStampCardReadDto(Guid Id, Guid PlayerId, int AccountingYear, List<StampReadDto> Stamps, string HolderName);

/// <summary>
/// A stamp as returned by the backend. The status is either <c>pending</c> or <c>accepted</c>.
/// </summary>
public record StampReadDto(Guid Id, string Reason, string Issuer, DateTimeOffset IssuedOn, string Status);

/// <summary>
/// A stamp submitted by an external user that waits for the approval of a coach, as returned by the backend.
/// The issuer is the holder of the share code the stamp was submitted with.
/// </summary>
public record PendingStampReadDto(Guid Id, string Reason, string Issuer, DateTimeOffset IssuedOn, Guid StampCardId, Guid PlayerId);

/// <summary>
/// A share code of a stamp card as returned by the backend. It is issued to a named player or parent, the holder.
/// A revoked code still grants read access, but stamps can no longer be submitted with it.
/// </summary>
public record SharedCodeReadDto(Guid Id, string Code, string HolderName, bool Revoked);
