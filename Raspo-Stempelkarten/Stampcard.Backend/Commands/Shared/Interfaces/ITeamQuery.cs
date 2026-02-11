using DispatchR.Abstractions.Send;

namespace StampCard.Backend.Commands.Shared.Interfaces;

/// <summary>
/// Base interface querying team data.
/// </summary>
public interface ITeamQuery : IRequest
{
    /// <summary>
    /// The team against which this query should be executed.
    /// </summary>
    public string Team { get; }
}