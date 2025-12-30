using DispatchR.Abstractions.Send;
using FluentResults;

namespace Raspo_Stempelkarten_Backend.Commands.StampCardDelete;

public record StampCardDeleteCommand(string Season, string Team, Guid Id, ulong? Version) 
    : IRequest<StampCardDeleteCommand, Task<Result>>;