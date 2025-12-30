using FluentResults;

namespace Raspo_Stempelkarten_Backend.Model;

public interface IStampCardAggregate
{
    Task<IEnumerable<StampCard>> GetStampCards();
    Task<Result<StampCard>> AddStampCard(
        string recipient, 
        string issuedBy,
        int minStamps, 
        int maxStamps,
        string[] additionalOwners);
    Task<Result<StampCard>> RemoveStampCard(Guid id, string issuedBy);
    Task<Result<(IEnumerable<string> AddedOwners, IEnumerable<string> RemovedOwners)>> SetStampCardOwners(
        Guid stampCardId, 
        string[] owners, 
        string issuedBy);
    Task<Result<Stamp>> Stamp(Guid stampCardId, string issuedBy, string? reason);
    Task<Result<Stamp>> EraseStamp(Guid stampCardId, Guid stampId, string issuedBy);
    Task<StampCard?> GetById(Guid id);
    public ulong? ConcurrencyToken { get; }
    Task<IEnumerable<StampCard>> List();
    Task<Stamp?> GetStampById(Guid stampCardId, Guid id);
    Task<IEnumerable<Stamp>> GetStamps(Guid id);
    Task<Result<StampCard>> Update(Guid id, string recipient, string issuer, 
        int minStamps, int maxStamps, string[] owners);
}