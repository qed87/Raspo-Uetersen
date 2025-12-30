using DispatchR.Abstractions.Send;
using FluentResults;

namespace Raspo_Stempelkarten_Backend.Commands.StampCardStampErase;

public record StampCardStampErasedCommand(string Season, string Team, Guid StampCardId, Guid Id) 
    : IRequest<StampCardStampErasedCommand, Task<Result<StampCardStampErasedResponse>>>;