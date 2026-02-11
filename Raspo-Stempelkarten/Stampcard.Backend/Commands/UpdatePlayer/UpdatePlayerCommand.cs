using DispatchR.Abstractions.Send;
using FluentResults;
using StampCard.Backend.Commands.Shared;
using StampCard.Backend.Commands.Shared.Interfaces;

namespace StampCard.Backend.Commands.UpdatePlayer;

/// <summary>
/// Command for update a player of the team.
/// </summary>
public record UpdatePlayerCommand(
    string Team,
    Guid Id,
    string FirstName,
    string LastName,
    DateOnly Birthdate,
    string Birthplace,
    ulong ConcurrencyToken,
    bool Active = true) : IRequest<UpdatePlayerCommand, Task<Result<Unit>>>, ITeamCommand, IConcurrentCommand;