using DispatchR.Abstractions.Send;

namespace Raspo_Stempelkarten_Backend.Commands.Shared;

/// <summary>
/// Application specific request interface. 
/// </summary>
public interface ITeamCommand : IRequest
{
    /// <summary>
    /// The team against which this command should be executed.
    /// </summary>
    public string Team { get; }
}