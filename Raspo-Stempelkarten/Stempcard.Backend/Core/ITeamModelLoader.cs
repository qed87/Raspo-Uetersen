using Raspo_Stempelkarten_Backend.Model;

namespace Raspo_Stempelkarten_Backend.Core;

/// <summary>
/// The team model loader.
/// </summary>
public interface ITeamModelLoader
{
    /// <summary>
    /// Loads the team model for the given stream.
    /// </summary>
    /// <param name="streamId">The stream id.</param>
    /// <param name="expectedVersion">The expected stream version.</param>
    /// <param name="position">The start position to read from.</param>
    Task<ITeamAggregate> LoadModelAsync(string streamId, long? expectedVersion = null, long? position = null);
}