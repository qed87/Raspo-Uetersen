using DispatchR.Abstractions.Send;

namespace StampCard.Backend.Commands.Shared.Interfaces;

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

/// <summary>
/// Signals that a command contains a concurrency token that should be checked during model loading.
/// </summary>
public interface IConcurrentCommand
{
    /// <summary>
    /// The concurrency token.
    /// </summary>
    public ulong ConcurrencyToken { get; }
}