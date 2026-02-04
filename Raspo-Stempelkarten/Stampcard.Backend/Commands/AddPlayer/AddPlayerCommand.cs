using DispatchR.Abstractions.Send;
using FluentResults;
using StampCard.Backend.Commands.Shared;

namespace StampCard.Backend.Commands.AddPlayer;

/// <summary>
/// Command for adding a new member to the team.
/// </summary>
public record AddPlayerCommand(string Team, string FirstName, string LastName, DateOnly Birthdate, string Birthplace)
    : IRequest<AddPlayerCommand, Task<Result<Guid>>>, ITeamCommand;