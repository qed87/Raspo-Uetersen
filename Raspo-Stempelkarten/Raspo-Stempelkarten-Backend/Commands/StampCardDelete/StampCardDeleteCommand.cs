using DispatchR.Abstractions.Send;
using FluentResults;

namespace Raspo_Stempelkarten_Backend.Commands.StampCardDelete;

public record StampCardDeleteCommand(string Team, string Season, Guid Id, ulong? Version) 
    : IRequest<StampCardDeleteCommand, Task<Result>>;