using Raspo_Stempelkarten_Backend.Model;

namespace Raspo_Stempelkarten_Backend.Commands.Shared;

public interface IStempelkartenModelLoader
{
    Task<StempelkartenAggregate> LoadModelAsync(
        string team,
        string season);
}