using FluentResults;
using LiteBus.Commands.Abstractions;

namespace Raspo_Stempelkarten_Backend.Commands.StempelkarteDelete;

public record StempelkartenDeleteCommand(string Team, string Season, Guid Id, ulong? Version) : ICommand<Result>;