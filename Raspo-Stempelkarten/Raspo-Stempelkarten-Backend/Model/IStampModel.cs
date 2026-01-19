using FluentResults;

namespace Raspo_Stempelkarten_Backend.Model;

public interface IStampModel
{
    ulong? Version { get; set; }
    List<Player> Players { get; }
    List<StampCard> Cards { get; }
    Result<Guid> AddPlayer(string firstName, string surname, DateOnly birthdate);
    Result<Guid> DeletePlayer(Guid playerId);
    Result<StampCard> AddStampCard(Guid issuedTo, short accountingYear);
    Result<Stamp> AddStamp(Guid stampCardId, string reason);
    Result<Stamp> EraseStamp(Guid stampCardId, Guid stampId);
    Result<List<StampCard>> CreateNewAccountingYear(int accountingYear);
    Result<List<StampCard>> GetIncompleteStampCards(int accountingYear, int numberOfRequiredStamps);
    Result<List<StampCard>> GetCompleteStampCards(int accountingYear, int numberOfRequiredStamps);
}