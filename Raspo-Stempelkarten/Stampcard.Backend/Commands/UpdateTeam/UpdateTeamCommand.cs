using DispatchR.Abstractions.Send;
using FluentResults;
using StampCard.Backend.Commands.Shared;
using StampCard.Backend.Commands.Shared.Interfaces;

namespace StampCard.Backend.Commands.UpdateTeam;

/// <summary>
/// Update a existing team name.
/// </summary>
public record UpdateTeamCommand(
    string Team,
    string Name,
    ulong ConcurrencyToken)
    : IRequest<UpdateTeamCommand, Task<Result<ulong>>>, ITeamCommand, IConcurrentCommand;
