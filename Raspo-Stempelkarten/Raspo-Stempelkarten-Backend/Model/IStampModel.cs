using FluentResults;

namespace Raspo_Stempelkarten_Backend.Model;

public interface IStampModel
{
    ulong? Version { get; set; }
    List<Player> Players { get; }
    Result<Guid> AddPlayer(string firstName, string surname, DateOnly birthdate);
    Result<Guid> DeletePlayer(Guid playerId);
}