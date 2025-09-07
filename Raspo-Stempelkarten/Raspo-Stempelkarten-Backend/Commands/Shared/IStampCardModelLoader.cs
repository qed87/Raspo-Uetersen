using Raspo_Stempelkarten_Backend.Model;

namespace Raspo_Stempelkarten_Backend.Commands.Shared;

public interface IStampCardModelLoader
{
    /// <summary>
    /// Load <see cref="StampCardAggregate"/> from storage.
    /// </summary>
    /// <param name="team">The team name.</param>
    /// <param name="season">The season.</param>
    /// <returns>The loaded <see cref="StampCardAggregate" />.</returns>
    Task<IStampCardAggregate> LoadModelAsync(
        string team,
        string season);
}