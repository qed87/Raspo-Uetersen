using FluentResults;

namespace Raspo_Stempelkarten_Backend.Model;

public interface IStampCardAggregate
{
    Task<IEnumerable<StampCard>> GetStampCards();
    Task<Result<StampCard>> AddStampCard(
        string recipient, 
        string issuedBy,
        int minStamps, 
        int maxStamps);
    Task<Result<StampCard>> RemoveStampCard(Guid id, string issuedBy);
    Task<Result<(IEnumerable<string> AddedOwners, IEnumerable<string> RemovedOwners)>> SetStampCardOwners(
        Guid stampCardId, 
        string[] owners, 
        string issuedBy);
    Task<Result<Stamp>> Stamp(Guid stampCardId, string issuedBy, string? reason);
    Task<Result<Stamp>> EraseStamp(Guid stampCardId, Guid stampId, string issuedBy);
    Task<StampCard?> GetById(Guid id);
    public ulong? ConcurrencyToken { get; }
}