using FluentResults;

namespace Raspo_Stempelkarten_Backend.Model;

/// <summary>
/// Interface for team model
/// </summary>
public interface ITeamAggregate
{
    /// <summary>
    /// The revision of the team.
    /// </summary>
    ulong? Version { get; set; }
    /// <summary>
    /// The id of the team.
    /// </summary>
    string Id { get; set; }
    /// <summary>
    /// Indicates whether the team is deleted.
    /// </summary>
    bool Deleted { get; set; }
    /// <summary>
    /// The players of the team.
    /// </summary>
    List<Player> Players { get; }
    /// <summary>
    /// The stamp cards of the team.
    /// </summary>
    List<StampCard> Cards { get; }
    /// <summary>
    /// The coaches of the team.
    /// </summary>
    List<Coach> Coaches { get; }
    /// <summary>
    /// Add a player to the team.
    /// </summary>
    /// <param name="firstName">The first name of the player.</param>
    /// <param name="surname">The surname of the player.</param>
    /// <param name="birthdate">The birthdate of the player.</param>
    /// <param name="birthplace">The birthplace of the player.</param>
    /// <return>The player id.</return>
    Task<Result<Guid>> AddPlayerAsync(string firstName, string surname, DateOnly birthdate, string birthplace);
    /// <summary>
    /// Deletes a player.
    /// </summary>
    /// <param name="playerId">The player id.</param>
    /// <returns>The id of the deleted player.</returns>
    Task<Result<Guid>> DeletePlayerAsync(Guid playerId);
    /// <summary>
    /// Adds a stamp card to the team.
    /// </summary>
    /// <param name="playerId">The player for that the stamp card was issued.</param>
    /// <param name="accountingYear">The accounting year.</param>
    /// <returns></returns>
    Task<Result<Guid>> AddStampCardAsync(Guid playerId, short accountingYear);
    /// <summary>
    /// Stamps a stamp card.
    /// </summary>
    /// <param name="stampCardId">The stamp card id.</param>
    /// <param name="reason">The reason for the stamp on the stamp card.</param>
    /// <returns>The created stamp.</returns>
    Task<Result<Guid>> StampStampCardAsync(Guid stampCardId, string reason);
    /// <summary>
    /// Erase a stamp from a stamp card.
    /// </summary>
    /// <param name="stampCardId">The stamp card.</param>
    /// <param name="stampId">The stamp to delete.</param>
    /// <returns>The erased stamp.</returns>
    Task<Result<Guid>> EraseStampAsync(Guid stampCardId, Guid stampId);
    /// <summary>
    /// Creates stamp cards for the given <paramref name="accountingYear"/> for all players. 
    /// </summary>
    /// <param name="accountingYear">The accounting year.</param>
    /// <returns>All the stamp cards.</returns>
    Task<Result> CreateNewAccountingYearAsync(int accountingYear);
    /// <summary>
    /// Determines all stamp cards that are incomplete for the given accounting year.
    /// </summary>
    /// <param name="accountingYear">The accounting year.</param>
    /// <param name="numberOfRequiredStamps">The minimum number of stamps per accounting year.</param>
    /// <returns>All stamp cards.</returns>
    Result<List<StampCard>> GetIncompleteStampCards(int accountingYear, int numberOfRequiredStamps);
    /// <summary>
    /// Determines the accounting year that are completed for the given accounting year.
    /// </summary>
    /// <param name="accountingYear">The accounting year.</param>
    /// <param name="numberOfRequiredStamps">The minimum number of stamps per accounting year.</param>
    /// <returns>All stamp cards.</returns>
    Result<List<StampCard>> GetCompleteStampCards(int accountingYear, int numberOfRequiredStamps);
    /// <summary>
    /// Delete the team.
    /// </summary>
    Task<Result<string>> DeleteTeamAsync();
    /// <summary>
    /// Delete the stamp card.
    /// </summary>
    /// <param name="id">The stamp card id.</param>
    /// <returns>The delete stamp card.</returns>
    Task<Result<Guid>> DeleteStampCard(Guid id);

    /// <summary>
    /// Adds a new coach to the team.
    /// </summary>
    /// <param name="email">The email of the coach.</param>
    /// <param name="issuedBy">The name of the issuer.</param>
    /// <returns></returns>
    Task<Result> AddCoach(string email, string issuedBy);

    /// <summary>
    /// Removes a coach to the team.
    /// </summary>
    /// <param name="email">The email of the coach.</param>
    /// <param name="issuedBy">The name of the issuer.</param>
    /// <returns></returns>
    Task<Result> RemoveCoach(string email, string issuedBy);
}