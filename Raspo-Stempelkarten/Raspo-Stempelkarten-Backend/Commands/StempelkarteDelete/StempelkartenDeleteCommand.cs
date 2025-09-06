using DispatchR.Abstractions.Send;
using FluentResults;

namespace Raspo_Stempelkarten_Backend.Commands.StempelkarteDelete;

public record StempelkartenDeleteCommand(string Team, string Season, Guid Id, ulong? Version) 
    : IRequest<StempelkartenDeleteCommand, Task<Result>>;