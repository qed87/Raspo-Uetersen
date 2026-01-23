using Raspo_Stempelkarten_Backend.Model;

namespace Raspo_Stempelkarten_Backend.Commands.Shared;

public interface IStampModelLoader
{
    Task<IStampModel> LoadModelAsync(string streamId);
}