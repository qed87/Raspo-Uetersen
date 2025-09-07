using DispatchR.Abstractions.Send;
using FluentResults;

namespace Raspo_Stempelkarten_Backend.Commands.StampCardStampErase;

public record StampCardStampErasedCommand(string Team, string Season, Guid StampCardId, Guid Id) 
    : IRequest<StampCardStampErasedCommand, Task<Result<StampCardStampErasedResponse>>>;